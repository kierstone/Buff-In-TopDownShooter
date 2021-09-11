using NUnit.Framework;

using Codice.Client.BaseCommands.Merge;
using Codice.Client.Commands;
using Codice.Client.Commands.Mount;
using Codice.CM.Common;
using Codice.CM.Common.Merge;
using PlasticGui.WorkspaceWindow.IncomingChanges;
using Unity.PlasticSCM.Editor.Views.IncomingChanges.Developer;

namespace Unity.PlasticSCM.Tests.Editor.Views.IncomingChanges.Developer
{
    [TestFixture]
    class IsResolvedTests
    {
        [Test]
        public void IsDirectoryConflictResolved()
        {
            IncomingChangeInfo dirConflict = BuilDirectoryConflict(true, 55);

            Assert.IsTrue(
                IsSolved.Conflict(dirConflict, null, null),
                "Conflict should be resolved");
        }

        [Test]
        public void IsDirectoryConflictNotResolved()
        {
            IncomingChangeInfo dirConflict = BuilDirectoryConflict(false, 55);

            Assert.IsFalse(
                IsSolved.Conflict(dirConflict, null, null),
                "Conflict shouldn't be resolved");
        }


        [Test]
        public void IsFileConflictResolved()
        {
            long itemId = 55;
            MountPointId mountPointId = MountPointId.WORKSPACE_ROOT;

            IncomingChangeInfo fileConflict = BuildFileConflict(mountPointId, itemId);

            MergeSolvedFileConflicts solvedFileConflicts = new MergeSolvedFileConflicts();
            solvedFileConflicts.AddResolveFile(mountPointId, itemId, "foo.c");

            Assert.IsTrue(
                IsSolved.Conflict(fileConflict, null, solvedFileConflicts),
                "Conflict should be resolved");
        }

        [Test]
        public void IsFileConflictNotResolved()
        {
            long itemId = 55;
            MountPointId mountPointId = MountPointId.WORKSPACE_ROOT;

            IncomingChangeInfo fileConflict = BuildFileConflict(mountPointId, itemId);

            MergeSolvedFileConflicts solvedFileConflicts = new MergeSolvedFileConflicts();

            Assert.IsFalse(
                IsSolved.Conflict(fileConflict, null, solvedFileConflicts),
                "Conflict shouldn't be resolved");
        }

        [Test]
        public void IsDirectoryConflictWithMetaResolved()
        {
            IncomingChangeInfo dirConflict = BuilDirectoryConflict(true, 55);
            IncomingChangeInfo metaDirConflict = BuilDirectoryConflict(true, 55);

            Assert.IsTrue(
                IsSolved.Conflict(dirConflict, metaDirConflict, null),
                "Conflict should be resolved");
        }

        [Test]
        public void IsDirectoryConflictWithMetaNotResolved()
        {
            IncomingChangeInfo dirConflict = BuilDirectoryConflict(true, 55);
            IncomingChangeInfo metaDirConflict = BuilDirectoryConflict(false, 66);

            Assert.IsFalse(
                IsSolved.Conflict(dirConflict, metaDirConflict, null),
                "Conflict shouldn't be resolved");
        }

        [Test]
        public void IsFileConflictWithMetaResolved()
        {
            long itemId = 55;
            long metaItemId = 66;

            MountPointId mountPointId = MountPointId.WORKSPACE_ROOT;

            IncomingChangeInfo fileConflict = BuildFileConflict(mountPointId, itemId);
            IncomingChangeInfo metaFileConflict = BuildFileConflict(mountPointId, metaItemId);

            MergeSolvedFileConflicts solvedFileConflicts = new MergeSolvedFileConflicts();
            solvedFileConflicts.AddResolveFile(mountPointId, itemId, "foo.c");
            solvedFileConflicts.AddResolveFile(mountPointId, metaItemId, "foo.c.meta");

            Assert.IsTrue(
                IsSolved.Conflict(fileConflict, metaFileConflict, solvedFileConflicts),
                "Conflict should be resolved");
        }

        [Test]
        public void IsFileDirectoryConflictWithMetaNotResolved()
        {
            long itemId = 55;
            long metaItemId = 66;

            MountPointId mountPointId = MountPointId.WORKSPACE_ROOT;

            IncomingChangeInfo fileConflict = BuildFileConflict(mountPointId, itemId);
            IncomingChangeInfo metaFileConflict = BuildFileConflict(mountPointId, metaItemId);

            MergeSolvedFileConflicts solvedFileConflicts = new MergeSolvedFileConflicts();
            solvedFileConflicts.AddResolveFile(mountPointId, itemId, "foo.c");

            Assert.IsFalse(
                IsSolved.Conflict(fileConflict, metaFileConflict, solvedFileConflicts),
                "Conflict shouldn't be resolved");
        }

        IncomingChangeInfo BuilDirectoryConflict(bool isResolved, long itemId)
        {
            DiffChanged src = new DiffChanged(
                CreateFileRevision(itemId), -1, "foo.c", -1,
                Difference.DiffNodeStatus.Added);

            DiffChanged dst = new DiffChanged(
                CreateFileRevision(itemId), -1, "foo.c", -1,
                Difference.DiffNodeStatus.Added);

            DirectoryConflict dirConflict = new EvilTwinConflict(src, dst);
            dirConflict.SetIsResolved(isResolved);

            IncomingChangeInfo result = new IncomingChangeInfo(
                new MountPointWithPath(
                    MountPointId.WORKSPACE_ROOT,
                    new RepositorySpec(),
                    "/"),
                dirConflict,
                null,
                null,
                IncomingChangesCategory.Type.DirectoryConflicts);

            return result;
        }

        IncomingChangeInfo BuildFileConflict(MountPointId mountPointId, long itemId)
        {
            DiffChanged src = new DiffChanged(
                CreateFileRevision(itemId), -1, "foo.c", -1,
                Difference.DiffNodeStatus.Changed);

            DiffChanged dst = new DiffChanged(
                CreateFileRevision(itemId), -1, "foo.c", -1,
                Difference.DiffNodeStatus.Changed);

            return new IncomingChangeInfo(
                new MountPointWithPath(
                    mountPointId,
                    new RepositorySpec(),
                    "/"),
                new FileConflict(src, dst),
                IncomingChangesCategory.Type.FileConflicts);
        }

        static RevisionInfo CreateFileRevision(long itemId)
        {
            RevisionInfo result = new RevisionInfo();
            result.Type = EnumRevisionType.enTextFile;
            result.Size = 128;
            result.ItemId = itemId;
            return result;
        }
    }
}
