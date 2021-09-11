using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.PlasticSCM.Editor.UI.UIElements
{
    internal class LoadingSpinner : VisualElement
    {
        internal LoadingSpinner()
        {
            mStarted = false;

            // add child elements to set up centered spinner rotation
            VisualElement spinner = new VisualElement();
            Add(spinner);

            spinner.style.backgroundImage = Images.GetImage(Images.Name.Loading);
            spinner.style.width = 16;
            spinner.style.height = 16;
            spinner.style.left = -8;
            spinner.style.top = -8;

            style.position = Position.Relative;
            style.width = 16;
            style.height = 16;
            style.left = 8;
            style.top = 8;
        }

        internal void Dispose()
        {
            if (mStarted)
                EditorApplication.update -= UpdateProgress;
        }

        internal void Start()
        {
            if (mStarted)
                return;

            mRotation = 0;
            mLastRotationTime = EditorApplication.timeSinceStartup;

            EditorApplication.update += UpdateProgress;

            mStarted = true;
        }

        internal void Stop()
        {
            if (!mStarted)
                return;

            EditorApplication.update -= UpdateProgress;

            mStarted = false;
        }

        void UpdateProgress()
        {
            double currentTime = EditorApplication.timeSinceStartup;
            double deltaTime = currentTime - mLastRotationTime;

            transform.rotation = Quaternion.Euler(0, 0, mRotation);
            mRotation += (int)(ROTATION_SPEED * deltaTime);
            mRotation = mRotation % 360;
            if (mRotation < 0) mRotation += 360;

            mLastRotationTime = currentTime;
        }

        int mRotation;
        double mLastRotationTime;
        bool mStarted;

        const int ROTATION_SPEED = 360; // Euler degrees per second
    }
}
