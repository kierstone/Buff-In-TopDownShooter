# Migration from Collaborate to Plastic SCM

Complete the steps to migrate Collaborate projects with the [Collaborate migration tool](https://www.plasticscm.com/plasticscm-cloud-edition/migrate-unity-projects/). You can invite your team members, set up your Plastic SCM workspace, and install Plastic SCM for Unity (beta).

 **Videos**

* [Migration Wizard](https://youtu.be/TKZuvPMprKg)
* [Plastic SCM Plugin Dev workflow](https://youtu.be/6_x3SLCiyWo)
* [Plastic SCM Plugin Gluon workflow](https://youtu.be/kfRu21cArGc)

**Migration Steps**

1. Access the [Collaborate migration tool](https://www.plasticscm.com/plasticscm-cloud-edition/migrate-unity-projects) on the Plastic SCM website.

2. Click on **Get Started** and complete the wizard to migrate your Collaborate project(s) to Plastic SCM:

   ![Migrate Collaborate projects](images/MigrateProjects.png)

3. Download and install the Plastic SCM [Cloud Client](https://www.plasticscm.com/download/).

4. Open the Plastic SCM Cloud Client and log in.

5. Select the Organization that you have created with the migration tool.

6. Select the workflow that fits your needs.

  * **Developer Workflow**

    With this workflow, you can work with branching and merging. To check in your changes straight to your cloud repository, create your workspace using the centralized tab.

    ![Developer workflow](images/DeveloperWorkflow.png)

* **Gluon Workflow**
  
    This workflow, tailored for artists, allows you to pick the files you want to work on and check them back in without updating your whole workspace. To work inside the Unity Editor, make sure to configure Gluon to pull down all Unity project files required to open a Unity project.

   ![Gluon workflow](images/GluonWorkflow.png)

7. Create a Workspace.

* If your Unity project files are already on your machine, select the directory path corresponding to your Unity project's root.
* If your Unity project files are not on your machine, choose any location in which to create your workspace.

8. Update or configure your Workspace.

* If you select the **Gluon Workflow** , configure your workspace to pull down all Unity project files required to open a Unity project.
* If you choose the **Developer Workflow** , update your workspace to ensure your project is up to date with all incoming changes.

9. Open your Unity project through the Hub.

   **Note:** Unity will add the ability to join projects directly from the Unity Hub in a future version.

10. Select **Window** &gt; **Plastic SCM**.

You're all set to work on your project using Plastic SCM!
