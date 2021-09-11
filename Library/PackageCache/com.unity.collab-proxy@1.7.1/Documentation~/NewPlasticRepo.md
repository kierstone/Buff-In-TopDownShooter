# Getting started with a new Plastic SCM repository

To start from an existing Plastic SCM repository, see [Getting started with an existing Plastic SCM repository](ExistingPlasticRepo.md).

1. Open your Unity project.
2. To access the Plastic SCM window in the Unity Editor, select **Window** &gt; **Plastic SCM**.
3. In the Plastic SCM onboarding window, complete the steps to continue.
4. Download and install the Plastic SCM edition that matches your license. For example, if you are using Plastic SCM for the Cloud, download the Cloud Edition.
5. Click on **Login or sign up.** You can **sign in using your Unity ID** or with a Plastic SCM account.
6. Create a Plastic SCM **Organization** (this is different from your Unity organization) for your repositories or join an existing one.

   ![Plastic SCM organization](images/PlasticOrg.png)
  
7. Create a **Workspace** for your project.

   ![Workspace creation](images/Workspace.png)

Your workspace interacts with the version control, where you download the files and make the required changes for each check in.

**Note:** You can have several workspaces working with the same local repository.

By default, Plastic SCM sets the repository name and workspace name to the name of your Unity project.

8. Choose the workflow you'd like to work with:

   **Plastic Workspace** : Great for branching, merging and allowing you to push/pull.
   
   **Gluon Workspace** : Choose this workflow If you only need to check in your work and sync with your teammates.

Unity connects your project to your Plastic SCM Cloud repository.

Plastic SCM automatically creates an ignore file in the workspace for Unity projects, so it doesn't track files that shouldn't be part of the repository. It also creates a standard automatic check in during the initial setup, so you're all set to start creating!

![Plastic SCM window](images/AutomaticSetup.png)
