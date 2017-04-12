using System.Collections;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using Cursor = HoloToolkit.Unity.InputModule.Cursor;
using System;
using HoloToolkitExtensions.Messaging;
using CubeBouncer.Messages;

namespace CubeBouncer
{
    public class CubeManipulator : MonoBehaviour, IInputClickHandler
    {
        private Rigidbody _rigidBody;

        private AudioSource _audioSource;

        public int ForceMultiplier = 100;

        public int Id;

        public AudioClip BounceTogetherClip;

        public AudioClip BounceOtherClip;

        public AudioClip ComeBackClip;

        public BaseRayStabilizer Stabilizer = null;

        private Vector3 _orginalPosition;

        private Vector3 _originalRotation;

        // Use this for initialization
        private void Start()
        {
            _rigidBody = GetComponent<Rigidbody>();
            _audioSource = GetComponent<AudioSource>();

            _orginalPosition = transform.position;
            _originalRotation = transform.rotation.eulerAngles;
        }

        public void OnInputClicked(InputClickedEventData eventData)
        {
            var ray = Stabilizer != null 
                ? Stabilizer.StableRay
                : new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            MoveByRay(ray);
        }

        public void Drop()
        {
            _rigidBody.useGravity = true;
        }

        private void MoveByRay(Ray ray)
        {
            _rigidBody.AddForceAtPosition(
                new Vector3(
                    ray.direction.x * ForceMultiplier,
                    ray.direction.y * ForceMultiplier,
                    ray.direction.z * ForceMultiplier),
                GazeManager.Instance.HitPosition);
        }

        public void Revert(bool doPlaySound)
        {
            if(_rigidBody.isKinematic)
            {
                return; // already returning
            }

            if (doPlaySound)
            {
                _audioSource.PlayOneShot(ComeBackClip);
            }
            _rigidBody.isKinematic = true;

            _rigidBody.useGravity = false;
            LeanTween.move(gameObject, _orginalPosition, 1f);
            LeanTween.rotate(gameObject, _originalRotation, 1f).setOnComplete(() => _rigidBody.isKinematic = false);
        }

        void OnCollisionEnter(Collision coll)
        {
            // Ignore returning bodies
            if (_rigidBody.isKinematic) return;

            // Ignore hits by cursors
            if (coll.gameObject.GetComponent<Cursor>() != null) return;

            // Play a click on hitting another cube, but only if the it has a higher Id
            // to prevent the same sound being played twice
            var othercube = coll.gameObject.GetComponent<CubeManipulator>();
            if (othercube != null && othercube.Id < Id)
            {
                _audioSource.PlayOneShot(BounceTogetherClip);
            }

            // No cursor, no cube - we hit a wall.
            if (othercube == null)
            {
                if (coll.relativeVelocity.magnitude > 0.1)
                {
                    _audioSource.PlayOneShot(BounceOtherClip);
                }
            }
        }

        // See http://answers.unity3d.com/questions/711309/movement-script-2.html
        IEnumerator MoveObject(Transform thisTransform, Vector3 startPos, Vector3 endPos,
            Quaternion startRot, Quaternion endRot, float time)
        {
            var i = 0.0f;
            var rate = 1.0f / time;
            while (i < 1.0f)
            {
                i += Time.deltaTime * rate;
                thisTransform.position = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0f, 1f, i));
                thisTransform.rotation = Quaternion.Lerp(startRot, endRot, Mathf.SmoothStep(0f, 1f, i));
                yield return null;
            }
        }
    }
}
