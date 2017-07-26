using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace Yemi
{
    [Serializable]
    public class CustomTextEditor
    {


        private static readonly Color m_colBg = new Color(0.3f, 0.3f, 0.3f);
        private static readonly Color m_colBgLine = new Color(0.32f, 0.32f, 0.32f);
        private static readonly Color m_colInfo = new Color(0.5f, 0.5f, 0.5f);
        private static readonly Color m_colLineCode = new Color(0.3f, 0.4f, 0.9f);
        private static readonly Color m_colText = new Color(0.9f, 0.9f, 0.9f);

        private static Rect m_mainRect = new Rect();
        private float m_lineHeight = 16f;
        private int m_line;

        private float m_lineCodeOff = 26f;

        private Rect m_infoRect;
        private float m_infoHeight = 20;
        private string m_infoContext = "";

        private Rect m_cursorPos;
        private static bool m_cursorShow = true;
        private static int m_cursorCount;


        private int m_selectLine = 0;
        private int m_selectCol = 0;
        private float m_selectXOff = 0;
        [SerializeField]
        private List<string> m_data = new List<string>();

        private GUIStyle m_textStyle;


        public void DrawGui(Rect rect)
        {
            m_mainRect.width = rect.width;
            m_mainRect.height = rect.height;

            m_infoRect.y = rect.height - m_infoHeight;
            m_infoRect.height = m_infoHeight;
            m_infoRect.width = rect.width;

            m_line = Mathf.FloorToInt(m_mainRect.height / m_lineHeight);

            ProcessInput();




            //do draw
            EditorGUI.DrawRect(m_mainRect, m_colBg);


            DrawTexts();


            DrawInfo();

            DrawCursor();
        }

        private void ProcessInput()
        {
            Event e = Event.current;

            switch(e.type)
            {
                case EventType.mouseDown:
                    EvtMouseDown(e.mousePosition);
                    break;
                case EventType.keyDown:
                    EvtKeyDown(e.keyCode,e);
                    break;
            }
        }


        #region Event
        private void EvtMouseDown(Vector2 pos)
        {
            m_selectLine = Mathf.FloorToInt(pos.y / m_lineHeight);

            var content = new GUIContent(m_data[m_selectLine]);
            var textRect = new Rect(m_lineCodeOff, m_selectLine * m_lineHeight, m_mainRect.width, m_lineHeight);
            int index = m_textStyle.GetCursorStringIndex(textRect, content, pos);

            var cpos = m_textStyle.GetCursorPixelPosition(textRect, content, index);
            m_selectXOff = cpos.x;
            m_selectCol = index;

        }

        private void EvtKeyDown(KeyCode code,Event e)
        {
            DataCheck();

            if((int)code >=97 && (int)code <=122)
            {
                string addc = e.shift?code.ToString():code.ToString().ToLower();
                m_data[m_selectLine] = m_data[m_selectLine].Insert(m_selectCol, addc);
                m_selectCol++;
                RefreshTextColPosition();
            }
            if(code == KeyCode.UpArrow)
            {
                CMDLineUp();
            }
            else if(code == KeyCode.DownArrow)
            {
                CMDLineDown();
            }
            else if(code == KeyCode.LeftArrow)
            {
                CMDLineLeft();
            }
            else if(code == KeyCode.RightArrow)
            {
                CMDLineRight();
            }
            else if(code == KeyCode.Delete)
            {
                CMDDeleteChar();
            }
        }


        #endregion

        private Rect GetTextRect() { return new Rect(m_lineCodeOff, m_selectLine * m_lineHeight, m_mainRect.width, m_lineHeight); }
        private void RefreshTextColPosition()
        {
            if (m_selectCol > m_data[m_selectLine].Length)
                m_selectCol = m_data[m_selectLine].Length;

            var cpos = m_textStyle.GetCursorPixelPosition(GetTextRect(), new GUIContent(m_data[m_selectLine]), m_selectCol);
            m_selectXOff = cpos.x;
        }

        private void CMDDeleteChar()
        {
            if(m_data[m_selectLine].Length == 0)
            {

            }
            else
            {
                if(m_selectCol > 0)
                {
                    var str = m_data[m_selectLine];
                    var strn = str.Substring(0, m_selectCol - 1) + str.Substring(m_selectCol);
                    ShowInfo(strn);
                    m_data[m_selectLine] = strn;
                    m_selectCol--;
                    RefreshTextColPosition();
                }
                else
                {

                }
            }
        }

        private void CMDLineUp()
        {
            m_selectLine--;
            m_selectLine = m_selectLine < 0 ? 0 : m_selectLine;
            RefreshTextColPosition();

        }
        private void CMDLineDown()
        {
            m_selectLine++;
            m_selectLine = m_selectLine > m_data.Count-1  ? m_data.Count - 1 : m_selectLine;

            RefreshTextColPosition();
        }

        private void CMDLineLeft()
        {
            if(m_selectCol ==0)
            {
                if(m_selectLine != 0)
                {
                    m_selectLine--;
                    m_selectCol = m_data[m_selectLine].Length;
                }
                else
                {
                    return;
                }
            }
            else
            {
                m_selectCol--;
            }

            RefreshTextColPosition();
        }

        private void CMDLineRight()
        {
            if(m_selectCol == m_data[m_selectLine].Length)
            {
                if(m_selectLine < m_data.Count-1)
                {
                    m_selectLine++;
                    m_selectCol = 0;
                }
                else
                {
                    return;
                }
            }
            else
            {
                m_selectCol++;
            }
            RefreshTextColPosition();
        }


        private void DrawTexts()
        {
            DataCheck();

            EditorGUI.DrawRect(new Rect(0, 0, m_lineCodeOff, m_mainRect.height), m_colLineCode);

            Rect lineRect = new Rect(m_mainRect);
            lineRect.height = m_lineHeight;
            Rect lineCodeRect = new Rect(lineRect);
            lineRect.x = m_lineCodeOff;

            for (int i=0;i<m_line;i++)
            {
                //line bg
                if (i % 2 == 1) EditorGUI.DrawRect(lineRect, m_colBgLine);

                //line code
                GUI.Label(lineCodeRect, (i+1).ToString());

                //text
                GUI.Label(lineRect, m_data[i], m_textStyle);

                lineRect.y += m_lineHeight;
                lineCodeRect.y += m_lineHeight;


            }
        }
        private void DrawCursor()
        {
            if (!m_cursorShow) return;

            m_cursorPos.x = m_selectXOff;
            m_cursorPos.y = m_lineHeight * m_selectLine;
            m_cursorPos.width = 2f;
            m_cursorPos.height = m_lineHeight;


            EditorGUI.DrawRect(m_cursorPos, Color.white);
        }
        private void DrawInfo()
        {
            EditorGUI.DrawRect(m_infoRect, m_colInfo);
            GUI.Label(m_infoRect, m_infoContext);
        }

        private void ShowInfo(object o)
        {
            m_infoContext = o.ToString();
        }

        private void DataCheck()
        {
            while(m_data.Count <=m_line)
            {
                m_data.Add("");
            }

            if (m_textStyle == null)
            {
                m_textStyle = new GUIStyle(GUI.skin.label);
                m_textStyle.normal.textColor = m_colText;
            }
        }

        public static void EditorUpdate()
        {
            //m_cursorCount++;
            //m_cursorCount = m_cursorCount % 10;
            //if(m_cursorCount == 0)
            //{
            //    m_cursorShow = !m_cursorShow;
            //}
        }


    }

}

