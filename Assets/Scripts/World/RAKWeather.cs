using UnityEngine;
using System.Collections;
using DigitalRuby.RainMaker;
using rak;

public class RAKWeather : MonoBehaviour
{
    private GameObject sun;
    private RAKCloud[] clouds;
    private WindZone windZone;
    private AudioSource audioSource;
    private GameObject rainPrefab;
    private AudioSource windAudio;
    private RAKAudioClip[] audioClips;
    private WeatherType currentWeather;
    private RAKTerrainMaster masterTerrain;
    private Material skyboxMat;
    private bool initialized = false;
    public float skyboxExposure { get; set; }

    public void start(Transform master, RAKTerrainMaster masterTerrain, GameObject sun)
    {
        this.sun = sun;
        this.masterTerrain = masterTerrain;
        windZone = master.gameObject.GetComponent<WindZone>();
        audioClips = new RAKAudioClip[2];
        audioClips[0] = new RAKAudioClip(RAKUtilities.getAudioClip(RAKUtilities.AUDIO_CLIP_RAIN_LIGHT));
        audioClips[1] = new RAKAudioClip(RAKUtilities.getAudioClip(RAKUtilities.AUDIO_CLIP_WIND_MEDIUM));
        rainPrefab = RAKUtilities.getPrefab("RainPrefab");
        audioSource = master.gameObject.AddComponent<AudioSource>();
        audioSource.clip = audioClips[0].audioClip;
        audioSource.loop = true;
        windAudio = master.gameObject.AddComponent<AudioSource>();
        windAudio.clip = audioClips[1].audioClip;
        windAudio.loop = true;
        setWeather(WeatherType.Clear);
        initialized = true;
    }
    public GameObject getPlayerRainPrefab()
    {
        return rainPrefab;
    }
    private void setWeather(WeatherType weather)
    {
        this.currentWeather = weather;
        if (currentWeather == WeatherType.Clear)
        {
            skyboxMat = RAKUtilities.getMaterial(RAKUtilities.MATERIAL_SKYBOX_SUNSET);
            skyboxMat.color = new Color32(164, 164, 164, 196);
            skyboxExposure = 1f;
            windAudio.volume = 0;
            windZone.windMain = 0;
            audioSource.volume = 0;
        }
        else if (currentWeather == WeatherType.Overcast)
        {
            skyboxMat = RAKUtilities.getMaterial(RAKUtilities.MATERIAL_SKYBOX_SUNSET);
            skyboxMat.color = new Color32(184, 255, 193, 202);
            skyboxExposure = 1.28f;
            windAudio.volume = .2f;
            audioSource.volume = .4f;
            windZone.mode = WindZoneMode.Directional;
            windZone.windMain = 1;
        }
        else if (currentWeather == WeatherType.DownPour)
        {
            skyboxMat = RAKUtilities.getMaterial(RAKUtilities.MATERIAL_SKYBOX_SUNSET);
            skyboxMat.color = new Color32(164, 164, 164, 196);
            skyboxExposure = 1;
            windAudio.volume = .5f;
            audioSource.volume = .8f;
            windZone.mode = WindZoneMode.Spherical;
            windZone.windMain = 2;
        }
        if (currentWeather != WeatherType.Clear)
        {
            generateClouds(masterTerrain.transform, masterTerrain.getTerrain(), currentWeather);
            audioSource.Play();
        }
        else
        {
            clouds = new RAKCloud[0];
        }
        windAudio.Play();
        skyboxMat.SetFloat("_Exposure", skyboxExposure);
        RenderSettings.skybox = skyboxMat;
        DynamicGI.UpdateEnvironment();
    }
    private void generateClouds(Transform master, RAKTerrain[] terrain, WeatherType weatherType)
    {
        int worldSize = masterTerrain.getSquareSize();
        int numberOfClouds;
        float distanceBetweenClouds;
        Vector2 cloudSize;
        int minSize;
        RAKCloud.CloudType cloudType;
        if (weatherType == WeatherType.Overcast)
        {
            numberOfClouds = worldSize / 20;
            cloudSize = new Vector2(8, 8);
            distanceBetweenClouds = 100;
            minSize = 2;
            cloudType = RAKCloud.CloudType.LIGHT_NORMAL;
        }
        else if (weatherType == WeatherType.DownPour)
        {
            numberOfClouds = 10;
            minSize = 1;
            cloudSize = new Vector2(1, 1);
            distanceBetweenClouds = 100;
            cloudType = RAKCloud.CloudType.DENSE_NORMAL;
        }
        else
        {
            minSize = 0;
            numberOfClouds = 0;
            cloudSize = Vector2.one;
            distanceBetweenClouds = 0;
            cloudType = RAKCloud.CloudType.NONE;
        }
        clouds = new RAKCloud[numberOfClouds];
        int y = 200;
        for (int count = 0; count < clouds.Length; count++)
        {
            int x = Random.Range(0, worldSize);
            int z = Random.Range(0, worldSize);
            if (isTooCloseToOther(new Vector3(x, y, z), clouds, distanceBetweenClouds))
            {
                count--;
            }
            else
            {
                clouds[count] = new RAKCloud(new Vector2((int)Random.Range(minSize, cloudSize.x), (int)Random.Range(minSize, cloudSize.y)), new Vector3(x, y, z), master, cloudType);
            }
        }
    }
    private bool isTooCloseToOther(Vector3 position, RAKCloud[] others, float minDistance)
    {
        for (int count = 0; count < others.Length; count++)
        {
            if (others[count] == null || others[count].cloudTransform == null) continue;
            if (Vector3.Distance(others[count].cloudTransform.position, position) < minDistance)
            {
                return true;
            }
        }
        return false;
    }
    private void Update()
    {
        if (!initialized) return;
        if (currentWeather != WeatherType.Clear)
        {
            for (int count = 0; count < clouds.Length; count++)
            {
                if (clouds[count] == null) continue;
                clouds[count].update();
                if (clouds[count].cloudTransform.localPosition.z >= clouds[count].destroyWhenReachedThis)
                {
                    Destroy(clouds[count].cloudTransform.gameObject);
                    clouds[count] = null;
                }
            }
        }
    }

    public WeatherType getCurrentWeather()
    {
        return currentWeather;
    }

    public class RAKCloud
    {
        public enum CloudType { DENSE_NORMAL, LIGHT_NORMAL, NONE }

        public Transform cloudTransform { get; set; }
        private WindZone wind;
        public Vector2 size { get; set; }
        private CloudType cloudType;
        public float destroyWhenReachedThis { get; set; }
        private GameObject cloudSection;

        public RAKCloud(Vector2 size, Vector3 position, Transform master, CloudType cloudType)
        {
            this.cloudType = cloudType;
            destroyWhenReachedThis = master.GetComponent<RAKTerrainMaster>().getSquareSize();
            this.size = size;
            GameObject cloudMaster = new GameObject("Cloud");
            cloudMaster.transform.SetParent(master);
            cloudMaster.transform.localPosition = position;
            this.cloudTransform = cloudMaster.transform;
            this.wind = master.GetComponent<WindZone>();
            if (cloudType != CloudType.NONE)
            {
                this.cloudSection = addCloudParticle(cloudType, cloudMaster);
            }

        }
        public GameObject addCloudParticle(CloudType cloudType, GameObject parent)
        {
            GameObject cloudSection;
            if (cloudType == CloudType.DENSE_NORMAL)
            {
                cloudSection = RAKUtilities.getPrefab("CloudSectionAngry");
            }
            else
            {
                cloudSection = null;
            }
            cloudSection = (GameObject)Instantiate(cloudSection, Vector3.zero, Quaternion.identity);//, 1);
            cloudSection.transform.SetParent(parent.transform);
            cloudSection.transform.localPosition = Vector3.zero;
            return cloudSection;
            /*ParticleSystem po = parent.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule mainModule = po.main;
            
            ParticleSystem.ShapeModule shapeModule = po.shape;
            ParticleSystem.MinMaxCurve emissionCurve = po.emission.rateOverTime;
            ParticleSystem.LimitVelocityOverLifetimeModule velOverTimeModule = po.limitVelocityOverLifetime;
            ParticleSystem.ColorOverLifetimeModule colorModule = po.colorOverLifetime;
            ParticleSystem.ColorBySpeedModule colorBySpeed = po.colorBySpeed;
            ParticleSystem.RotationOverLifetimeModule rotationModule = po.rotationOverLifetime;
            ParticleSystem.NoiseModule noiseModule = po.noise;
            po.Stop();
            if (cloudType == CloudType.DENSE_NORMAL)
            {
                ParticleSystem.MinMaxCurve startSize = new ParticleSystem.MinMaxCurve(25);
                mainModule.startSize = startSize;
                mainModule.startRotationX = 0;
                mainModule.startRotationY = 360;
                mainModule.maxParticles = 5000;
                mainModule.duration = 10;
                mainModule.loop = true;
                mainModule.prewarm = true;
                mainModule.startLifetime = 10;
                mainModule.gravityModifier = .4f;
                mainModule.simulationSpace = ParticleSystemSimulationSpace.World;
                mainModule.simulationSpeed = .2f;
                mainModule.startSpeed = 0;
                mainModule.scalingMode = ParticleSystemScalingMode.Shape;
                mainModule.emitterVelocityMode = ParticleSystemEmitterVelocityMode.Transform;
                ParticleSystem.MinMaxGradient startGradient = new ParticleSystem.MinMaxGradient(new Color(79, 79, 79, 255), new Color(161, 161, 161, 255));
                mainModule.startColor = startGradient;

                shapeModule.shapeType = ParticleSystemShapeType.Box;
                shapeModule.scale = new Vector3(10, 0, 10);
                shapeModule.randomDirectionAmount = 0;
                shapeModule.randomPositionAmount = 0;

                emissionCurve.constant = 500;

                velOverTimeModule.limit = 50;
                velOverTimeModule.dampen = .1f;
                velOverTimeModule.enabled = true;

                Gradient colorOverLifeGradient = new Gradient();
                GradientColorKey[] colorOverLifeColorKeys = new GradientColorKey[2];
                colorOverLifeColorKeys[0] = new GradientColorKey(new Color(128, 128, 128), 0);
                colorOverLifeColorKeys[1] = new GradientColorKey(new Color(45, 95, 118), 100);
                colorOverLifeGradient.SetKeys(colorOverLifeColorKeys, new GradientAlphaKey[] { new GradientAlphaKey(255, 0) });
                colorModule.color = colorOverLifeGradient;
                colorModule.enabled = true;

                Gradient speedGradient = new Gradient();
                GradientColorKey[] speedColorKeys = new GradientColorKey[3];
                speedColorKeys[0] = new GradientColorKey(new Color(45, 95, 118), 0);
                speedColorKeys[1] = new GradientColorKey(new Color(8, 46, 63), 40);
                speedColorKeys[2] = new GradientColorKey(new Color(0, 0, 0), 100);
                speedGradient.SetKeys(speedColorKeys, new GradientAlphaKey[] { new GradientAlphaKey(255, 0) });
                colorBySpeed.color = speedGradient;
                colorBySpeed.range = new Vector2(0, 50);
                colorBySpeed.enabled = true;

                rotationModule.x = 75;
                rotationModule.y = 75;
                rotationModule.z = 75;
                rotationModule.enabled = true;

                noiseModule.strength = 10;
                noiseModule.frequency = .05f;
                noiseModule.scrollSpeed = 1;
                noiseModule.damping = true;
                noiseModule.octaveCount = 4;
                noiseModule.octaveMultiplier = .5f;
                noiseModule.octaveScale = 2;
                noiseModule.enabled = true;
            }
            else if (cloudType == CloudType.LIGHT_NORMAL)
            {
                ParticleSystem.MinMaxCurve startSize = new ParticleSystem.MinMaxCurve(15, 25);
                mainModule.startSize = startSize;
                shapeModule.scale = new Vector3(15, 0, 15);
                emissionCurve.constant = 150;
                mainModule.maxParticles = 450;
                shapeModule.randomDirectionAmount = 1;
                shapeModule.randomPositionAmount = 1;
            }
            po.Play();
            return po;*/
        }

        public void update()
        {
            cloudTransform.position += (wind.windMain * wind.transform.forward) * Time.deltaTime;
        }
    }
    public enum WeatherType { DownPour, Overcast, Clear }
}
