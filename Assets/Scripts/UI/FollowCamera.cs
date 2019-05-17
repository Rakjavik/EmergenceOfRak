using rak.world;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace rak.UI
{
    public class FollowCamera : MonoBehaviour
    {
        private static Vector3 _defaultOffset = Vector3.one*10;
        private static int speed = 3;
        private static float mouseLookSpeed = 1f;
        private static float mouseWheelZoomModifier = 1;
        private static float maxZoomIn = .5f;
        private static float maxZoomOut = 30;
        private static Transform _target = null;
        private static Vector3 cameraOffset = Vector3.one;
        private static float targetZoomDistance = _defaultOffset.z;
        private float _ignoreInputs = 0f;
        private Vector3 _lastMousePosition;
        private bool _moveWithMouse = false;
        private bool _stopCameraMovement = false;

        public static void SetFollowTarget(Transform target)
        {
            _target = target;
            cameraOffset = _defaultOffset;
        }

        private void Awake()
        {
            _lastMousePosition = Input.mousePosition;
        }
        // Update is called once per frame
        private void toggleMovement()
        {
            _stopCameraMovement = !_stopCameraMovement;
        }
        void Update()
        {
            if (!World.Initialized) return;
            if (Input.GetKeyUp(KeyCode.Space))
            {
                toggleMovement();
            }
            if (_target == null)
            {
                if (CreatureBrowserMono.SelectedCreature == null)
                    return;
                _target = CreatureBrowserMono.SelectedCreature.transform;
                transform.position = _target.position + cameraOffset;
            }
            else
            {
                if (_stopCameraMovement) return;
                transform.position = Vector3.Lerp(transform.position, _target.position+cameraOffset, Time.deltaTime*speed);
                transform.LookAt(_target);
                Vector3 positionForward;
                float currentDistance = Vector3.Distance(transform.position, _target.position);
                // Too small of a difference to alter //
                float precision = .1f;
                if (Mathf.Abs(currentDistance - targetZoomDistance) < precision)
                    return;
                if (currentDistance > targetZoomDistance)
                {
                    positionForward = transform.forward * Time.deltaTime;
                }
                else
                {
                    positionForward = -transform.forward * Time.deltaTime;
                }
                cameraOffset += positionForward;
            }
            if (_ignoreInputs <= 0f)
            {
                Vector3 distanceMouseMoved = _lastMousePosition - Input.mousePosition;
                _lastMousePosition = Input.mousePosition;

                float mouseWheelAmount = Input.GetAxis("Mouse ScrollWheel");
                if (Mathf.Abs(mouseWheelAmount) > 0f)
                {
                    targetZoomDistance -= mouseWheelAmount*mouseWheelZoomModifier;
                    DebugMenu.AppendLine("Current zoom - " + targetZoomDistance);
                    if (targetZoomDistance < maxZoomIn)
                        targetZoomDistance = maxZoomIn;
                    else if (targetZoomDistance > maxZoomOut)
                        targetZoomDistance = maxZoomOut;
                }
                // Middle mouse button //
                else if (Input.GetMouseButtonDown(2))
                {
                    cameraOffset = _defaultOffset;
                    targetZoomDistance = _defaultOffset.z;
                    transform.position = _target.position + _defaultOffset;
                    transform.LookAt(_target.position);
                }
                // Left Mouse //
                if (Input.GetMouseButtonDown(0))
                {
                    _moveWithMouse = true;
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    if (_moveWithMouse) _moveWithMouse = false;
                }
                // Right Mouse //
                else if (Input.GetMouseButton(1))
                {

                }
                if(distanceMouseMoved != Vector3.zero && _moveWithMouse)
                {
                    cameraOffset += transform.right * distanceMouseMoved.x*mouseLookSpeed*Time.deltaTime;
                    cameraOffset += transform.up * distanceMouseMoved.y*mouseLookSpeed*Time.deltaTime;
                }
            }
            else
            {
                _ignoreInputs -= Time.deltaTime;
            }
        }
    }
}
