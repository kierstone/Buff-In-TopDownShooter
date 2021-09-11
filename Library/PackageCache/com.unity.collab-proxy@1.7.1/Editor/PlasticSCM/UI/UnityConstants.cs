namespace Unity.PlasticSCM.Editor.UI
{
    internal static class UnityConstants
    {
        internal const float CANCEL_BUTTON_SIZE = 15;

        internal const float SMALL_BUTTON_WIDTH = 40;
        internal const float REGULAR_BUTTON_WIDTH = 60;
        internal const float LARGE_BUTTON_WIDTH = 100;
        internal const float EXTRA_LARGE_BUTTON_WIDTH = 130;

        internal const float SEARCH_FIELD_WIDTH = 550;

        internal const string TREEVIEW_META_LABEL = " +meta";
        internal const float TREEVIEW_CHECKBOX_SIZE = 17;
        internal const float TREEVIEW_BASE_INDENT = 16f;
        internal const float FIRST_COLUMN_WITHOUT_ICON_INDENT = 5;
#if UNITY_2019_1_OR_NEWER
        internal const float DROPDOWN_ICON_Y_OFFSET = 2;
        internal const float TREEVIEW_FOLDOUT_Y_OFFSET = 0;
        internal const float TREEVIEW_ROW_HEIGHT = 21;
        internal const float TREEVIEW_HEADER_CHECKBOX_Y_OFFSET = 0;
        internal const float TREEVIEW_CHECKBOX_Y_OFFSET = 0;
        internal static float DIR_CONFLICT_VALIDATION_WARNING_LABEL_HEIGHT = 21;
#else
        internal const float DROPDOWN_ICON_Y_OFFSET = 5;
        internal const float TREEVIEW_FOLDOUT_Y_OFFSET = 1;
        internal const float TREEVIEW_ROW_HEIGHT = 20;
        internal const float TREEVIEW_HEADER_CHECKBOX_Y_OFFSET = 6;
        internal const float TREEVIEW_CHECKBOX_Y_OFFSET = 2;
        internal static float DIR_CONFLICT_VALIDATION_WARNING_LABEL_HEIGHT = 18;
#endif

#if UNITY_2020_1_OR_NEWER
        internal const float INSPECTOR_ACTIONS_BACK_RECTANGLE_TOP_MARGIN = -2;
#else
        internal const float INSPECTOR_ACTIONS_BACK_RECTANGLE_TOP_MARGIN = 0;
#endif

#if UNITY_2019
        internal const int INSPECTOR_ACTIONS_HEADER_BACK_RECTANGLE_HEIGHT = 24;
#else
        internal const int INSPECTOR_ACTIONS_HEADER_BACK_RECTANGLE_HEIGHT = 7;
#endif


        internal const int LEFT_MOUSE_BUTTON = 0;
        internal const int RIGHT_MOUSE_BUTTON = 1;

        internal const int UNSORT_COLUMN_ID = -1;

        internal const string PLASTIC_WINDOW_TITLE = "Plastic SCM";
        internal const string LOGIN_WINDOW_TITLE = "Sign in to Plastic SCM";

        internal const int ACTIVE_TAB_UNDERLINE_HEIGHT = 2;
        internal const int SPLITTER_INDICATOR_HEIGHT = 1;

        internal const double SEARCH_DELAYED_INPUT_ACTION_INTERVAL = 0.25;
        internal const double SELECTION_DELAYED_INPUT_ACTION_INTERVAL = 0.25;
        internal const double AUTO_REFRESH_DELAYED_INTERVAL = 0.25;
        internal const double AUTO_REFRESH_PENDING_CHANGES_DELAYED_INTERVAL = 0.1;

        internal const string PENDING_CHANGES_TABLE_SETTINGS_NAME = "{0}_PendingChangesTreeV2_{1}";
        internal const string GLUON_INCOMING_CHANGES_TABLE_SETTINGS_NAME = "{0}_GluonIncomingChangesTree_{1}";
        internal const string GLUON_INCOMING_ERRORS_TABLE_SETTINGS_NAME = "{0}_GluonIncomingErrorsList_{1}";
        internal const string GLUON_UPDATE_REPORT_TABLE_SETTINGS_NAME = "{0}_GluonUpdateReportList_{1}";
        internal const string DEVELOPER_INCOMING_CHANGES_TABLE_SETTINGS_NAME = "{0}_DeveloperIncomingChangesTree_{1}";
        internal const string DEVELOPER_UPDATE_REPORT_TABLE_SETTINGS_NAME = "{0}_DeveloperUpdateReportList_{1}";
        internal const string REPOSITORIES_TABLE_SETTINGS_NAME = "{0}_RepositoriesList_{1}";
        internal const string CHANGESETS_TABLE_SETTINGS_NAME = "{0}_ChangesetsList_{1}";
        internal const string CHANGESETS_DATE_FILTER_SETTING_NAME = "{0}_ChangesetsDateFilter_{1}";
        internal const string CHANGESETS_SHOW_CHANGES_SETTING_NAME = "{0}_ShowChanges_{1}";
        internal const string HISTORY_TABLE_SETTINGS_NAME = "{0}_HistoryList_{1}";

        internal static class ChangesetsColumns
        {
            internal const float CHANGESET_NUMBER_WIDTH = 80;
            internal const float CHANGESET_NUMBER_MIN_WIDTH = 50;
            internal const float CREATION_DATE_WIDTH = 150;
            internal const float CREATION_DATE_MIN_WIDTH = 100;
            internal const float CREATED_BY_WIDTH = 200;
            internal const float CREATED_BY_MIN_WIDTH = 110;
            internal const float COMMENT_WIDTH = 300;
            internal const float COMMENT_MIN_WIDTH = 100;
            internal const float BRANCH_WIDTH = 160;
            internal const float BRANCH_MIN_WIDTH = 90;
            internal const float REPOSITORY_WIDTH = 210;
            internal const float REPOSITORY_MIN_WIDTH = 90;
            internal const float GUID_WIDTH = 270;
            internal const float GUID_MIN_WIDTH = 100;
        }
    }
}
