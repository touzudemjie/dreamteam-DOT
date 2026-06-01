using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.VirtualTexturing;


namespace UsefulClasses
{
    public enum Tag
    {
        Player
    }
    public readonly struct TransformSnapshot
    {
        public readonly Vector3 position;
        public readonly Quaternion rotation;
        public readonly float deltaTime;
        public TransformSnapshot(Vector3 position, Quaternion rotation, float deltaTime)
        {
            this.position = position;
            this.rotation = rotation;
            this.deltaTime = deltaTime;
        }
    }
    public readonly struct CameraSnapshot
    {
        public readonly Vector2 velocity;
        public readonly float timeDeltaTime;

        public CameraSnapshot(Vector2 mousePosition, float timeDeltaTime)
        {
            this.velocity = mousePosition;
            this.timeDeltaTime = timeDeltaTime;
        }
    }
    public enum GameScene
    {
        KamoPhysics,
        KamoStatic,
        Portal,
        ReverseTimer
    }
    public static class PlayerInputHandler
    {
        public enum PlayerAction
        {
            Move,
            Look,
            Attack,
            Interact,
            Crouch,
            Jump,
            Pause,
            Previous,
            Next,
            Pick,
            Sprint
        }
        public static InputAction GetInputAction(PlayerAction action)
        {
            return InputSystem.actions.FindAction(action.ToString());
        }

    }
    [System.Serializable]
    public class UnityTimer
    {
        [SerializeField] private float _minDuration;
        [SerializeField] private float _duration;
        [SerializeField] private bool _hasRandomRange;
        private float _currentTime;
        private bool _canRun;

        public UnityTimer(float duration)
        {
            this._duration = duration;
            PrepareStart();
        }

        public void PrepareStart()
        {
            if (_hasRandomRange)
            {
                _currentTime = Random.Range(_minDuration, _duration);
            }
            else
            {
                _currentTime = _duration;
            }
            _canRun = true;
        }

        public void Tick()
        {
            if (_canRun)
            {
                _currentTime -= Time.deltaTime;
                if (_currentTime <= 0)
                {
                    _canRun = false;
                }
            }
        }
        public void TickUnscaled()
        {
            if (_canRun)
            {
                _currentTime -= Time.unscaledDeltaTime;
                if (_currentTime <= 0)
                {
                    _canRun = false;
                }
            }
        }

        public bool IsFinished() => !_canRun && _currentTime <= 0;
        public bool IsRunning() => _canRun;
        public float Progress() => 1 - (_currentTime / _duration);
        public void Reset() => _currentTime = _duration;
        public float GetDuration() { return _duration; }
        public void SetDuration(float maxDuration, float minDuration = 0f)
        {
            _minDuration = minDuration;
            _duration = maxDuration;
        }
        public void Stop() => _canRun = false;
        public float GiveRandomLength() => UnityEngine.Random.Range(0, _duration);
    }

    public static class Helpers
    {
        private static Camera _camera;

        public static Camera Camera
        {
            get
            {
                if (_camera == null)
                    _camera = Camera.main;
                return _camera;
            }
            set 
            {
               _camera = value; 
            }
        }
        public static bool IsPointerOverGameObject()
        {
            // Schickt einen Raycast durch das UI-System (nicht Physics.Raycast!)
            var pointerData = new PointerEventData(EventSystem.current)
            {
                position = Mouse.current.position.ReadValue()
            };
           
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results); // UI Raycast

            return results.Count > 0; // trifft irgendein UI Element?
        }
        public static Vector3 GetMouseWorldPositionWithoutZ()
        {
            Vector3 mousePosition = Camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            mousePosition.z = 0;
            return mousePosition;
        }
        public static RectTransform GetUIElement()
        {
            // Schickt einen Raycast durch das UI-System (nicht Physics.Raycast!)
            var pointerData = new PointerEventData(EventSystem.current)
            {
                position = Mouse.current.position.ReadValue()
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results); // UI Raycast

            if (results.Count > 0) 
            {
                return results[0].gameObject.GetComponent<RectTransform>();
            }
            return null;
        }

        public static Vector3 GetWorldPositionOfCanvasElement(RectTransform elementTr, float zDepth)
        {
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, elementTr.position);
            // null weil Overlay keine Kamera braucht

            Ray ray = Camera.ScreenPointToRay(screenPos);
            return ray.GetPoint(zDepth); // zDepth = wie weit vor der Kamera
        }
        public static void DeleteChildren(this Transform transform)
        {
            foreach(Transform child in transform)
            {
                Object.Destroy(child.gameObject);
            }
        }
        
        private static readonly Dictionary<float, WaitForSeconds> _waitDictonary = new Dictionary<float, WaitForSeconds>();

        public static WaitForSeconds GetWait(float waitSeconds)
        {
            if(_waitDictonary.TryGetValue(waitSeconds, out WaitForSeconds wait)) return wait;
            _waitDictonary[waitSeconds] = new WaitForSeconds(waitSeconds);
            return _waitDictonary[waitSeconds];
        }
    }

}

