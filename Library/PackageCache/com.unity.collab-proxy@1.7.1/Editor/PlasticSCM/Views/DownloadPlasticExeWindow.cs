using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

using Codice.CM.Common;
using PlasticGui;
using Unity.PlasticSCM.Editor.UI.UIElements;
using Unity.PlasticSCM.Editor.Views.Welcome;
using Codice.Client.Common;
using Unity.PlasticSCM.Editor.Tool;

namespace Unity.PlasticSCM.Editor.Views
{
    internal class DownloadPlasticExeWindow :
        EditorWindow
    {
        internal static void ShowWindow(bool isGluonMode)
        {
            sIsGluonMode = isGluonMode;

            DownloadPlasticExeWindow window = GetWindow<DownloadPlasticExeWindow>();

            window.titleContent = new GUIContent(
                PlasticLocalization.GetString(PlasticLocalization.Name.PlasticSCM));

            if (EditionToken.IsCloudEdition())
                window.minSize = window.maxSize = new Vector2(700, 160);
            else
                window.minSize = window.maxSize = new Vector2(700, 230);

            window.Show();
        }

        void OnEnable()
        {
            BuildComponents();
            mInstallerFile = GetInstallerTmpFileName.ForPlatform();
        }

        void OnDestroy()
        {
            Dispose();
        }

        void Dispose()
        {
            mDownloadCloudEditionButton.clicked -= DownloadCloudEditionButton_Clicked;
            if (!EditionToken.IsCloudEdition())
                mDownloadEnterpriseButton.clicked -= DownloadEnterpriseEditionButton_Clicked;
            mCancelButton.clicked -= CancelButton_Clicked;
            EditorApplication.update -= CheckForPlasticExe;
        }

        void DownloadCloudEditionButton_Clicked()
        {
            DownloadAndInstallOperation.Run(
                Edition.Cloud,
                mInstallerFile,
                mProgressControls);

            EditorApplication.update += CheckForPlasticExe;
        }

        void DownloadEnterpriseEditionButton_Clicked()
        {
            DownloadAndInstallOperation.Run(
                Edition.Enterprise,
                mInstallerFile,
                mProgressControls);
        }

        void CancelButton_Clicked()
        {
            Close();
        }

        void CheckForPlasticExe()
        {
            // executable becomes available halfway through the install
            // we do not want to say install is done too early
            // when progress control finishes, cancel button will be enabled
            // then we can check for exe existing
            if (mCancelButton.enabledSelf && IsExeAvailable.ForMode(sIsGluonMode))
            {
                mMessageLabel.text = "Plastic SCM installed. You can now use the feature.";
                mCancelButton.text =
                    PlasticLocalization.GetString(PlasticLocalization.Name.CloseButton);
                mRequireMessageLabel.AddToClassList("display-none");
                mDownloadCloudEditionButton.AddToClassList("display-none");
                mDownloadEnterpriseButton.AddToClassList("display-none");
            }
        }

        void BuildComponents()
        {
            VisualElement root = rootVisualElement;
            root.Clear();
            InitializeLayoutAndStyles();

            mRequireMessageLabel = root.Q<Label>("requireMessage");
            mMessageLabel = root.Q<Label>("message");
            mDownloadCloudEditionButton = root.Q<Button>("downloadCloudEdition");
            mDownloadEnterpriseButton = root.Q<Button>("downloadEnterpriseEdition");
            mCancelButton = root.Q<Button>("cancel");
            mProgressControlsContainer = root.Q<VisualElement>("progressControlsContainer");

            root.Q<Label>("title").text =
                PlasticLocalization.GetString(PlasticLocalization.Name.InstallPlasticSCM);

            mDownloadCloudEditionButton.text =
                PlasticLocalization.GetString(PlasticLocalization.Name.DownloadCloudEdition);
            mDownloadCloudEditionButton.clicked += DownloadCloudEditionButton_Clicked;

            if (EditionToken.IsCloudEdition())
            {
                mDownloadEnterpriseButton.AddToClassList("display-none");
                DownloadPlasticExeWindow window = GetWindow<DownloadPlasticExeWindow>();
            }
            else
            {
                mMessageLabel.text =
                    PlasticLocalization.GetString(
                        PlasticLocalization.Name.WhichVersionInstall);
                mDownloadEnterpriseButton.text =
                    PlasticLocalization.GetString(
                        PlasticLocalization.Name.DownloadEnterpriseEdition);
                mDownloadEnterpriseButton.clicked += DownloadEnterpriseEditionButton_Clicked;
            }

            mCancelButton.text =
                PlasticLocalization.GetString(PlasticLocalization.Name.CancelButton);
            mCancelButton.clicked += CancelButton_Clicked;

            mProgressControls = new ProgressControlsForDialogs(
                new VisualElement[] {
                    mDownloadCloudEditionButton,
                    mDownloadEnterpriseButton,
                    mCancelButton
                });

            mProgressControlsContainer.Add(mProgressControls);
        }

        void InitializeLayoutAndStyles()
        {
            rootVisualElement.LoadLayout(typeof(DownloadPlasticExeWindow).Name);
            rootVisualElement.LoadStyle(typeof(DownloadPlasticExeWindow).Name);
        }

        static bool sIsGluonMode;

        string mInstallerFile;

        Label mRequireMessageLabel;
        Label mMessageLabel;
        Button mDownloadCloudEditionButton;
        Button mDownloadEnterpriseButton;
        Button mCancelButton;
        VisualElement mProgressControlsContainer;
        ProgressControlsForDialogs mProgressControls;
    }
}