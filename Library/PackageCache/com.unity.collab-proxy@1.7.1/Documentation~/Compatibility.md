# Asset Store Plugin &amp; Unity Package Compatibility

Although the [Asset Store Plugin](https://assetstore.unity.com/packages/tools/utilities/plastic-scm-plugin-for-unity-beta-169442) and the Unity Package (in the package manager) are both available, you can only have one of the two installed.

Below is what happens if you install both the package and plugin version of the Plastic SCM window:

![Console errors](images/Compatibility.png)

The errors in the console indicate that you've imported both the Plastic Plugin from the Asset Store and the Version Control package.

To resolve these errors, remove either version from the project.
