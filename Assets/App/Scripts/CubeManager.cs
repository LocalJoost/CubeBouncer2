using UnityEngine;
using System.Collections.Generic;
using HoloToolkit.Unity.InputModule;
using HoloToolkitExtensions.Utilities;
using HoloToolkitExtensions.Messaging;
using CubeBouncer.Messages;
using System;

namespace CubeBouncer
{
    public class CubeManager : MonoBehaviour
    {
        public GameObject Cube;

        private bool _distanceMeasured;
        private float _lastInitTime;
        private readonly List<GameObject> _cubes = new List<GameObject>();

        public AudioClip ReadyClip;

        public BaseRayStabilizer Stabilizer = null;
        

        public AudioClip ReturnAllClip;

        private AudioSource _audioSource;


        // Use this for initialization
        void Start()
        {
            _distanceMeasured = false;
            _lastInitTime = Time.time;
            _audioSource = GetComponent<AudioSource>();
            Messenger.Instance.AddListener<CreateNewGridMessage>(p=> CreateNewGrid());
            Messenger.Instance.AddListener<DropMessage>( ProcessDropMessage);
            Messenger.Instance.AddListener<RevertMessage>(ProcessRevertMessage);
        }

        public void CreateNewGrid()
        {
            foreach (var c in _cubes)
            {
                Destroy(c);
            }
            _cubes.Clear();

            _distanceMeasured = false;
            _lastInitTime = Time.time;
        }

        public void RevertAll()
        {
            _audioSource.PlayOneShot(ReturnAllClip);
            foreach (var c in _cubes)
            {
                c.GetComponent<CubeManipulator>().Revert(false);
            }
        }
        public void DropAll()
        {
            foreach (var c in _cubes)
            {
                c.GetComponent<CubeManipulator>().Drop();
            }
        }

        private void ProcessDropMessage(DropMessage msg)
        {
            if(msg.All)
            {
                DropAll();
            }
            else
            {
                var lookedAt = GetLookedAtObject();
                if( lookedAt != null)
                {
                    lookedAt.Drop();
                }
            }
        }

        private void ProcessRevertMessage(RevertMessage msg)
        {
            if (msg.All)
            {
                RevertAll();
            }
            else
            {
                var lookedAt = GetLookedAtObject();
                if (lookedAt != null)
                {
                    lookedAt.Revert(true);
                }
            }
        }

        private CubeManipulator GetLookedAtObject()
        {
            if(GazeManager.Instance.IsGazingAtObject)
            {
                return GazeManager.Instance.HitInfo.collider.gameObject.GetComponent<CubeManipulator>();
            }

            return null;
        }

        // Update is called once per frame
        void Update()
        {
            if (!_distanceMeasured)
            {
                if (GazeManager.Instance.IsGazingAtObject)
                {
                    _distanceMeasured = true;
                    CreateGrid(GazeManager.Instance.HitPosition);
                }
                else
                {
                    // If we can't find a wall in 10 seconds, create a default grid 
                    if (Time.time > _lastInitTime + 10)
                    {
                        _distanceMeasured = true;
                        CreateGrid(LookingDirectionHelpers.CalculatePositionDeadAhead(3.5f));
                    }
                }
            }
        }

        private void CreateGrid(Vector3 hitPosition)
        {
            _audioSource.PlayOneShot(ReadyClip);

            var gazeOrigin = Camera.main.transform.position;
            var rotation = Camera.main.transform.rotation;

            var maxDistance = Vector3.Distance(gazeOrigin, hitPosition);

            transform.position = hitPosition;
            transform.rotation = rotation;

            var id = 0;

            const float size = 0.2f;
            var maxZ = maxDistance - 1f;
            const float maxX = 0.35f;
            const float maxY = 0.35f;
            var z = 1.5f;
            do
            {
                var x = -maxX;
                do
                {
                    var y = -maxY;
                    do
                    {
                        CreateCube(id++,
                            gazeOrigin + transform.forward * z +
                            transform.right * x +
                            transform.up * y,
                            rotation);
                        y += size;
                    } while (y <= maxY);
                    x += size;
                } while (x <= maxX);
                z += size;
            } while (z <= maxZ);
        }

        private void CreateCube(int id, Vector3 location, Quaternion rotation)
        {
            var c = Instantiate(Cube, location, rotation);
            //Rotate around it's own up axis so up points TO the camera
            c.transform.RotateAround(location, transform.up, 180f);
            c.transform.parent = transform;
            var m = c.GetComponent<CubeManipulator>();
            m.Stabilizer = Stabilizer;
            m.Id = id;
            _cubes.Add(c);
        }
    }
}
