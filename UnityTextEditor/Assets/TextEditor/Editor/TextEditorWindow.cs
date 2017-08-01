using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;

namespace Yemi
{
    [Serializable]
    public class TextEditorWindow : EditorWindow
    {
        [SerializeField]
        private CustomTextEditor m_textEditor;
        private int m_updateCount = 0;

        [MenuItem("Window/TextEditor")]
        public static void OpenWindow()
        {
            GetTextEditor();
        }

        public static TextEditorWindow GetTextEditor()
        {
            var window = EditorWindow.GetWindow<TextEditorWindow>();
            window.CheckResources();
            window.Show();

            return window;
        }

        static TextEditorWindow()
        {
            EditorApplication.update += EditorUpdate;
        }

        private static void EditorUpdate()
        {
            CustomTextEditor.EditorUpdate();
        }

        private void CheckResources()
        {
            if (m_textEditor == null)
            {
                m_textEditor = new CustomTextEditor();
            }
        }

        private void OnEnable()
        {
            Debug.Log("onEnable");

            CheckResources();

            m_textEditor.CMDOpenFile();
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


        public void OpenFile(string filepath)
        {
            m_textEditor.CMDOpenFile(filepath);
        }

        [OnOpenAsset]
        public static bool OpenAssets(int instanceId,int line)
        {

            string path = AssetDatabase.GetAssetPath(instanceId);
            if(path.EndsWith(".txt"))
            {
                GetTextEditor().OpenFile(path);
                return true;
            }
            return false;
        } 
    }

}

