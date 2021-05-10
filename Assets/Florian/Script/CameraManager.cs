using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Florian
{
    public class CameraManager : MonoBehaviour
    {
        [Header("Initial Transform")]
        public Vector3 _initPos;
        public Vector3 _initRot;

        float _rotationX = 0f;
        float _rotationY = 0f;

        public void CameraJoystickRotation(float horizontal, float vertical, float rotationSpeed, Transform target)
        {
            _rotationX += horizontal * rotationSpeed;
            _rotationY += vertical * rotationSpeed;
            _rotationY = Mathf.Clamp(_rotationY, -31f, 60f);

            transform.LookAt(target.position);
            target.rotation = Quaternion.Euler(_rotationY, _rotationX, 0f);
        }

        public void CameraRotateAroundPlayer(float offsetX, float offsetY, Transform target)
        {
            _rotationX += offsetX;
            _rotationY += offsetY;

            transform.LookAt(target.position);
            target.rotation = Quaternion.Euler(_rotationY, _rotationX, 0f);
        }

        public void CameraMovePosition(Vector3 endPos, float time)
        {
            StartCoroutine(CameraPosition(endPos, time));
        }

        public void CameraMovePosition(Vector3 offset, float time, bool useOffset)
        {
            if (useOffset)
                StartCoroutine(CameraPositionWithOffset(offset, time));
        }

        public void CameraMoveRotation(Vector3 endRot, float time)
        {
            StartCoroutine(CameraRotation(endRot, time));
        }

        public void CameraMoveRotation(Vector3 offset, float time, bool useOffset)
        {
            if (useOffset)
                StartCoroutine(CameraRotationWithOffset(offset, time));
        }

        public void CameraMove(Vector3 endPos, Vector3 endRot, float time)
        {
            StartCoroutine(MoveCamera(endPos, endRot, time));
        }

        public void CameraMove(Vector3 offsetPos, Vector3 offsetRot, float time, bool useOffset)
        {
            if (useOffset)
                StartCoroutine(MoveCameraWithOffset(offsetPos, offsetRot, time));
        }

        public void ResetTransform(float time, Transform target)
        {
            StartCoroutine(CameraReset(time, target));
        }

        public void ResetTarget(float time, Transform target)
        {
            StartCoroutine(TargetTransformReset(time, target));
        }

        IEnumerator TargetTransformReset(float time, Transform target)
        {
            Vector3 currentRotCameraRoot = target.localRotation.eulerAngles;
            float timepassed = 0;
            while (timepassed < time)
            {
                target.localRotation = Quaternion.Lerp(Quaternion.Euler(currentRotCameraRoot), Quaternion.Euler(Vector3.zero), timepassed / time);
                timepassed += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }

        IEnumerator CameraReset(float time, Transform target)
        {
            Vector3 currentRotCameraRoot = target.localRotation.eulerAngles;
            Vector3 currentRot = transform.localRotation.eulerAngles;
            Vector3 currentPos = transform.localPosition;
            float timepassed = 0;
            while (timepassed < time)
            {
                transform.localRotation = Quaternion.Lerp(Quaternion.Euler(currentRot), Quaternion.Euler(_initRot), timepassed / time);
                transform.localPosition = Vector3.Lerp(currentPos, _initPos, timepassed / time);
                target.localRotation = Quaternion.Lerp(Quaternion.Euler(currentRotCameraRoot), Quaternion.Euler(Vector3.zero), timepassed / time);
                timepassed += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }

        IEnumerator CameraPositionWithOffset(Vector3 offset, float time)
        {
            Vector3 initPos = transform.localPosition;
            float timepassed = 0;
            while (timepassed < time)
            {
                transform.localPosition = Vector3.Lerp(initPos, initPos + offset, timepassed / time);
                timepassed += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }

        IEnumerator CameraPosition(Vector3 endPos, float time)
        {
            Vector3 initPos = transform.localPosition;
            float timepassed = 0;
            while (timepassed < time)
            {
                transform.localPosition = Vector3.Lerp(initPos, endPos, timepassed / time);
                timepassed += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }

        IEnumerator CameraRotationWithOffset(Vector3 offset, float time)
        {
            Vector3 initRot = transform.localRotation.eulerAngles;
            float timepassed = 0;
            while (timepassed < time)
            {
                transform.localRotation = Quaternion.Lerp(Quaternion.Euler(initRot), Quaternion.Euler(initRot + offset), timepassed / time);
                timepassed += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }

        IEnumerator CameraRotation(Vector3 endRot, float time)
        {
            Vector3 initRot = transform.localRotation.eulerAngles;
            float timepassed = 0;
            while (timepassed < time)
            {
                transform.localRotation = Quaternion.Lerp(Quaternion.Euler(initRot), Quaternion.Euler(endRot), timepassed / time);
                timepassed += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }

        IEnumerator MoveCameraWithOffset(Vector3 offsetPos, Vector3 offsetRot, float time)
        {
            Vector3 initRot = transform.localRotation.eulerAngles;
            Vector3 initPos = transform.localPosition;
            float timepassed = 0;
            while (timepassed < time)
            {
                transform.localRotation = Quaternion.Lerp(Quaternion.Euler(initRot), Quaternion.Euler(initRot + offsetRot), timepassed / time);
                transform.localPosition = Vector3.Lerp(initPos, initPos + offsetPos, timepassed / time);
                timepassed += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }

        IEnumerator MoveCamera(Vector3 endPos, Vector3 endRot, float time)
        {
            Vector3 initRot = transform.localRotation.eulerAngles;
            Vector3 initPos = transform.localPosition;
            float timepassed = 0;
            while (timepassed < time)
            {
                transform.localRotation = Quaternion.Lerp(Quaternion.Euler(initRot), Quaternion.Euler(endRot), timepassed / time);
                transform.localPosition = Vector3.Lerp(initPos, endPos, timepassed / time);
                timepassed += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }
    }
}