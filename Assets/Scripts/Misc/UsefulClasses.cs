using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.VirtualTexturing;


namespace UsefulClasses
{
    public enum Tag
    {
        Player,
        Choices
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
        D_SceneTransition,
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
    public class UnityTimer : ISerializationCallbackReceiver
    {
        [field: SerializeField] public float MinDuration { get; private set; }
        [SerializeField] public float Duration;
        [SerializeField] private bool _hasRandomRange;
        public float CurrentTime { get; private set; }
        public bool CanRun {get; private set; }
        public bool IsRunning => CanRun && CurrentTime > 0;
        public event Action OnTimerFinished;
        public UnityTimer(float duration)
        {
            this.Duration = duration;
        }
        public void PrepareStart()
        {
            if (_hasRandomRange)
            {
                CurrentTime = UnityEngine.Random.Range(MinDuration, Duration);
            }
            else
            {
                CurrentTime = Duration;
            }
            CanRun = true;
        }

        public void Tick()
        {
            if (CanRun)
            {
                CurrentTime -= Time.deltaTime;
                if (CurrentTime <= 0)
                {
                    CanRun = false;
                }
            }
        }
        public void TickUnscaled()
        {
            if (CanRun)
            {
                CurrentTime -= Time.unscaledDeltaTime;
                if (CurrentTime <= 0)
                {
                    CanRun = false;
                }
            }
        }
        public bool IsFinished() => !CanRun && CurrentTime <= 0;
        public bool IsFinishedAndReset()
        {
           if(!CanRun && CurrentTime <= 0)
            {
                PrepareStart();
                OnTimerFinished?.Invoke();
                return true;
            }
            else
            {
                return false;
            }
        }
        public float Progress()
        {
            if (Duration == 0)
            {
                return -1;
            }
            else
            {
                return Mathf.Clamp01(1 - (CurrentTime / Duration));
            }
        }
        public void Reset()
        {
            CurrentTime = Duration;
            CanRun = true;
        }
        public void SetDuration(float maxDuration, float minDuration = 0f)
        {
            MinDuration = minDuration;
            Duration = maxDuration;
        }
        public void Stop() => CanRun = false;
        public void Resume() => CanRun = true;
        public float GiveRandomLength() => UnityEngine.Random.Range(MinDuration, Duration);

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            PrepareStart();
        }
    }
    public static class ReflectionExtensions
    {
        public static object[] GetAllFields(this object obj)
        {
            FieldInfo[] fields = obj.GetType().GetFields(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );

            List<object> result = new List<object>(fields.Length);
            foreach (FieldInfo field in fields)
            {
                result.Add(field.GetValue(obj));
            }
            return result.ToArray();
        }
    }
    public static class CollectionExtension
    {
        public static bool IsEmpty<T>(this ICollection<T> collection)
        {
            if (collection.Count == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
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
                UnityEngine.Object.Destroy(child.gameObject);
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

