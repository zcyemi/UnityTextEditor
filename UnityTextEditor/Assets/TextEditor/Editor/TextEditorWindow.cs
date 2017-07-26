using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace Yemi
{
    [Serializable]
    public class TextEditorWindow : EditorWindow
    {
        [SerializeField]
        private static CustomTextEditor m_textEditor;
        private int m_updateCount = 0;

        [MenuItem("Window/TextEditor")]
        public static void OpenWindow()
        {
            CheckResources();

            var window = EditorWindow.GetWindow<TextEditorWindow>();
            window.Show();
            
        }
        static TextEditorWindow()
        {
            EditorApplication.update += EditorUpdate;
        }

        private static void EditorUpdate()
        {
            CustomTextEditor.EditorUpdate();
        }

        private static void CheckResources()
        {
            if (m_textEditor == null)
            {
                m_textEditor = new CustomTextEditor();
            }
        }

        private void OnGUI()
        {
            CheckResources();


            
            m_textEditor.DrawGui(this.position);
        }

        private void Update()
        {
            m_updateCount++;
            m_updateCount = m_updateCount % 10;
            if ((m_updateCount % 10) == 9)
            {
                this.Repaint();
            }
        }
    }

}

