namespace GameFrame.Flux
{
    public partial class ActionType
    {
        [ActionType("OpenUI", "load the panel/popUp/child. panel will hide the prev panel,include popUps which above on that panel")]
        public const string OPEN_UI = nameof(OPEN_UI);
        [ActionType("CloseUI", "close the panel/popUp , and refresh last panel/popUp")]
        public const string CLOSE_UI = nameof(CLOSE_UI);

        [ActionType("ShowUI", "displays the target canvasGroup")]
        public const string SHOW_UI = nameof(SHOW_UI);
        [ActionType("ShowUI", "hide the target canvasGroup")]
        public const string HIDE_UI = nameof(HIDE_UI);

        [ActionType("Undo","")]
        public const string UNDO = nameof(UNDO);
        [ActionType("Undoable","")]
        public const string UNDOABLE = nameof(UNDOABLE);
        [ActionType("BeforeUndo","will sent before undo")]
        public const string BEFORE_UNDO = nameof(BEFORE_UNDO);

        [ActionType("SetCanvasOrder","set canvas order")]
        public const string SET_CANVAS_ORDER = nameof(SET_CANVAS_ORDER);
    }
}