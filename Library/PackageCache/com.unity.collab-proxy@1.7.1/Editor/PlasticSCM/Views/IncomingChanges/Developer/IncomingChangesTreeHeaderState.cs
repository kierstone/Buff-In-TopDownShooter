using System;
using System.Collections.Generic;

using UnityEditor.IMGUI.Controls;
using UnityEngine;

using PlasticGui;
using Unity.PlasticSCM.Editor.UI.Tree;

namespace Unity.PlasticSCM.Editor.Views.IncomingChanges.Developer
{
    internal enum IncomingChangesTreeColumn
    {
        Path,
        Size,
        Author,
        Details,
        Resolution,
        DateModififed
    }

    [Serializable]
    internal class IncomingChangesTreeHeaderState : MultiColumnHeaderState, ISerializationCallbackReceiver
    {
        internal static IncomingChangesTreeHeaderState GetDefault()
        {
            return new IncomingChangesTreeHeaderState(BuildColumns());
        }

        internal static List<string> GetColumnNames()
        {
            List<string> result = new List<string>();
            result.Add(PlasticLocalization.GetString(PlasticLocalization.Name.PathColumn));
            result.Add(PlasticLocalization.GetString(PlasticLocalization.Name.SizeColumn));
            result.Add(PlasticLocalization.GetString(PlasticLocalization.Name.CreatedByColumn));
            result.Add(PlasticLocalization.GetString(PlasticLocalization.Name.DetailsColumn));
            result.Add(PlasticLocalization.GetString(PlasticLocalization.Name.ResolutionMethodColumn));
            result.Add(PlasticLocalization.GetString(PlasticLocalization.Name.DateModifiedColumn));
            return result;
        }

        internal static string GetColumnName(IncomingChangesTreeColumn column)
        {
            switch (column)
            {
                case IncomingChangesTreeColumn.Path:
                    return PlasticLocalization.GetString(PlasticLocalization.Name.PathColumn);
                case IncomingChangesTreeColumn.Size:
                    return PlasticLocalization.GetString(PlasticLocalization.Name.SizeColumn);
                case IncomingChangesTreeColumn.Author:
                    return PlasticLocalization.GetString(PlasticLocalization.Name.AuthorColumn);
                case IncomingChangesTreeColumn.Details:
                    return PlasticLocalization.GetString(PlasticLocalization.Name.DetailsColumn);
                case IncomingChangesTreeColumn.Resolution:
                    return PlasticLocalization.GetString(PlasticLocalization.Name.ResolutionMethodColumn);
                case IncomingChangesTreeColumn.DateModififed:
                    return PlasticLocalization.GetString(PlasticLocalization.Name.DateModifiedColumn);
                default:
                    return null;
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (mHeaderTitles != null)
                TreeHeaderColumns.SetTitles(columns, mHeaderTitles);

            if (mColumsAllowedToggleVisibility != null)
                TreeHeaderColumns.SetVisibilities(columns, mColumsAllowedToggleVisibility);
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
        }

        static Column[] BuildColumns()
        {
            return new Column[]
            {
                new Column()
                {
                    width = 450,
                    headerContent = new GUIContent(
                        GetColumnName(IncomingChangesTreeColumn.Path)),
                    minWidth = 200,
                    allowToggleVisibility = false,
                },
                new Column()
                {
                    width = 150,
                    headerContent = new GUIContent(
                        GetColumnName(IncomingChangesTreeColumn.Size)),
                    minWidth = 45
                },
                new Column()
                {
                    width = 150,
                    headerContent = new GUIContent(
                        GetColumnName(IncomingChangesTreeColumn.Author)),
                    minWidth = 80
                },
                new Column()
                {
                    width = 200,
                    headerContent = new GUIContent(
                        GetColumnName(IncomingChangesTreeColumn.Details)),
                    minWidth = 100
                },
                new Column()
                {
                    width = 250,
                    headerContent = new GUIContent(
                        GetColumnName(IncomingChangesTreeColumn.Resolution)),
                    minWidth = 120
                },
                new Column()
                {
                    width = 330,
                    headerContent = new GUIContent(
                        GetColumnName(IncomingChangesTreeColumn.DateModififed)),
                    minWidth = 100
                }
            };
        }

        IncomingChangesTreeHeaderState(Column[] columns)
            : base(columns)
        {
            if (mHeaderTitles == null)
                mHeaderTitles = TreeHeaderColumns.GetTitles(columns);

            if (mColumsAllowedToggleVisibility == null)
                mColumsAllowedToggleVisibility = TreeHeaderColumns.GetVisibilities(columns);
        }

        [SerializeField]
        string[] mHeaderTitles;

        [SerializeField]
        bool[] mColumsAllowedToggleVisibility;
    }
}
