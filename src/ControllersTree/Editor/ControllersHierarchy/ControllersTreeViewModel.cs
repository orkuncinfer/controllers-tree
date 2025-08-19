using System;
using Playtika.Controllers;
using UnityEditor;
using UnityEngine;

namespace Playtika.Controllers.Editor
{
    public class ControllersTreeViewModel
    {
        internal const float SEPARATOR_SIZE = 4f;

        private const float INTERNAL_SEPARATOR_SIZE = SEPARATOR_SIZE * 5;
        private const float WINDOW_HEIGHT_ADJUSTMENT = 75;

        internal event Action<IControllerDebugInfo> CreateNewTreeTab;

        internal GUIStyle ToolbarStyle { get; private set; }
        internal GUIStyle SearchStyle { get; private set; }
        internal GUIStyle CancelSearchStyle { get; private set; }
        internal GUIStyle CloseButtonStyle { get; private set; }

        internal bool IsBottomPanelVisible { get; private set; }
        internal bool IsRightPanelVisible { get; private set; }
        internal float HierarchyWidth => _windowSize.x * _hierarchyPanelWidthPercent;
        internal float RightPanelWidth => _windowSize.x * _rightPanelWidthPercent;
        internal float TopPanelHeight => _windowSize.y * _topPanelHeightPercent;
        internal float BottomPanelHeight => _windowSize.y * _bottomPanelHeightPercent;
        internal string[] IgnoredMethods { get; private set; }

        private Vector2 _windowSize;
        private bool _isDraggable;
        private bool _isHorizontal;
        private float _hierarchyPanelWidthPercent;
        private float _rightPanelWidthPercent;
        private float _topPanelHeightPercent;
        private float _bottomPanelHeightPercent;

        internal void LoadGuiSkin()
        {
            SearchStyle = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle("ToolbarSearchTextField");
            CancelSearchStyle = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle("ToolbarSearchCancelButton");
            if (SearchStyle == null || CancelSearchStyle == null)
            {
                // support for Unity versions with bug
                SearchStyle = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle("ToolbarSeachTextField");
                CancelSearchStyle = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle("ToolbarSeachCancelButton");
            }
            ToolbarStyle = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle("Toolbar");
            CloseButtonStyle = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle("WinBtnCloseMac");
            if (CloseButtonStyle == null)
            {
                // Fallback options for Unity 6.0 and newer versions
                CloseButtonStyle = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle("WinBtnInactiveMac") ??
                                   GUI.skin.button;
            }
        }

        internal void InvokeCreateNewTreeTab(IControllerDebugInfo controller)
        {
            CreateNewTreeTab?.Invoke(controller);
        }

        internal void SwitchRightPanelState()
        {
            IsRightPanelVisible = !IsRightPanelVisible;
        }

        internal void SwitchBottomPanelState()
        {
            IsBottomPanelVisible = !IsBottomPanelVisible;
        }

        internal void SetWindowSize(Vector2 size)
        {
            _windowSize = size - Vector2.up * WINDOW_HEIGHT_ADJUSTMENT;
            if (IsRightPanelVisible)
            {
                if (RightPanelWidth <= 0)
                {
                    var percent = 0.75f - INTERNAL_SEPARATOR_SIZE / size.x;
                    _hierarchyPanelWidthPercent = percent;
                    _rightPanelWidthPercent = 1f - percent;
                }
                else
                {
                    _hierarchyPanelWidthPercent = 1f - _rightPanelWidthPercent - INTERNAL_SEPARATOR_SIZE / size.x;
                }
            }
            else
            {
                _hierarchyPanelWidthPercent = 0;
            }

            if (IsBottomPanelVisible)
            {
                if (BottomPanelHeight <= 0)
                {
                    _topPanelHeightPercent = 0.75f;
                    _bottomPanelHeightPercent = 0.25f;
                }
            }
            else
            {
                _topPanelHeightPercent = 0;
            }
        }

        internal void SetVerticalSeparatorRect(Rect verticalSeparatorRect)
        {
            EditorGUIUtility.AddCursorRect(verticalSeparatorRect, MouseCursor.ResizeHorizontal);
            var newMousePosition = Event.current.mousePosition;

            if (Event.current.type == EventType.MouseDown && verticalSeparatorRect.Contains(newMousePosition))
            {
                _isDraggable = true;
                _isHorizontal = false;
            }

            if (_isDraggable && !_isHorizontal)
            {
                var percent = Mathf.Clamp(Event.current.mousePosition.x / _windowSize.x, 0.25f, 0.75f) - INTERNAL_SEPARATOR_SIZE / _windowSize.x;
                _hierarchyPanelWidthPercent = percent;
                _rightPanelWidthPercent = 1f - percent;
            }

            if (Event.current.type == EventType.MouseUp)
            {
                _isDraggable = false;
            }
        }

        internal void SetHorizontalSeparatorRect(Rect horizontalSeparatorRect)
        {
            EditorGUIUtility.AddCursorRect(horizontalSeparatorRect, MouseCursor.ResizeVertical);
            var newMousePosition = Event.current.mousePosition;

            if (Event.current.type == EventType.MouseDown && horizontalSeparatorRect.Contains(newMousePosition))
            {
                _isDraggable = true;
                _isHorizontal = true;
            }

            if (_isDraggable && _isHorizontal)
            {
                var percent = Mathf.Clamp(Event.current.mousePosition.y / _windowSize.y, 0.25f, 0.75f) - INTERNAL_SEPARATOR_SIZE / _windowSize.y;
                _topPanelHeightPercent = percent;
                _bottomPanelHeightPercent = 1f - percent;
            }

            if (Event.current.type == EventType.MouseUp)
            {
                _isDraggable = false;
            }
        }

        internal void SetIgnoredMethods(string[] ignoredMethods)
        {
            IgnoredMethods = ignoredMethods;
        }
    }
}
