using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

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

        private int m_render_startLine = 0;

        private float m_lineCodeOff = 26f;

        private Rect m_infoRect;
        private float m_infoHeight = 20;
        private string m_infoContext = "";

        private Rect m_cursorPos;
        private static bool m_cursorShow = true;
        private static int m_cursorCount;

        private Rect m_scrollerRect = new Rect(0,0,20,0);
        private Rect m_scrollerHandleRect;


        private int m_selectLine = 0;
        private int m_selectCol = 0;
        private float m_selectXOff = 0;
        [SerializeField]
        private List<string> m_data = new List<string>();

        private GUIStyle m_textStyle;

        [SerializeField]
        public string m_filePath;


        //keymap
        private static readonly Dictionary<KeyCode, string> KEYMAP = new Dictionary<KeyCode, string>
        {
            {KeyCode.Alpha0,"0" },
            {KeyCode.Alpha1,"1" },
            {KeyCode.Alpha2,"2" },
            {KeyCode.Alpha3,"3" },
            {KeyCode.Alpha4,"4" },
            {KeyCode.Alpha5,"5" },
            {KeyCode.Alpha6,"6" },
            {KeyCode.Alpha7,"7" },
            {KeyCode.Alpha8,"8" },
            {KeyCode.Alpha9,"9" },
            {KeyCode.Minus,"-" },
            {KeyCode.Equals,"=" },
            {KeyCode.BackQuote,"`" },
            {KeyCode.Comma,"," },
            {KeyCode.Period,"." },
            {KeyCode.Slash,"/" },
            {KeyCode.Semicolon,";" },
            {KeyCode.Colon,"'" },
            {KeyCode.LeftBracket,"[" },
            {KeyCode.RightBracket,"]" },
            {KeyCode.Backslash,"\\" },
        };

        private static readonly Dictionary<KeyCode, string> KEYMAP_SHIFT = new Dictionary<KeyCode, string>
        {
            {KeyCode.Alpha0,")" },
            {KeyCode.Alpha1,"!" },
            {KeyCode.Alpha2,"@" },
            {KeyCode.Alpha3,"#" },
            {KeyCode.Alpha4,"$" },
            {KeyCode.Alpha5,"%" },
            {KeyCode.Alpha6,"^" },
            {KeyCode.Alpha7,"&" },
            {KeyCode.Alpha8,"*" },
            {KeyCode.Alpha9,"(" },
            {KeyCode.Minus,"_" },
            {KeyCode.Equals,"+" },
            {KeyCode.BackQuote,"~" },
            {KeyCode.Comma,"<" },
            {KeyCode.Period,">" },
            {KeyCode.Slash,"?" },
            {KeyCode.Semicolon,":" },
            {KeyCode.Colon,"\"" },
            {KeyCode.LeftBracket,"{" },
            {KeyCode.RightBracket,"}" },
            {KeyCode.Backslash,"|" },
        };



        public void DrawGui(Rect rect)
        {
            m_mainRect.width = rect.width;
            m_mainRect.height = rect.height;

            m_infoRect.y = rect.height - m_infoHeight;
            m_infoRect.height = m_infoHeight;
            m_infoRect.width = rect.width;

            m_line = Mathf.FloorToInt(m_mainRect.height / m_lineHeight);

            m_scrollerRect.height = rect.height;
            m_scrollerRect.width = 10;
            m_scrollerRect.x = rect.width - 10;
            m_scrollerHandleRect = m_scrollerRect;

            ProcessInput();




            //do draw
            EditorGUI.DrawRect(m_mainRect, m_colBg);


            DrawTexts();

            DrawScroller();

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
                case EventType.ScrollWheel:
                    EventScrollWheel(e);
                    break;

            }
        }


        #region Event
        private void EvtMouseDown(Vector2 pos)
        {
            m_selectLine = Mathf.FloorToInt(pos.y / m_lineHeight) + m_render_startLine;
            if(m_selectLine >= m_data.Count)
            {
                m_selectLine = m_data.Count - 1;
            }
            var content = new GUIContent(m_data[m_selectLine]);
            var textRect = new Rect(m_lineCodeOff, (m_selectLine - m_render_startLine) * m_lineHeight, m_mainRect.width, m_lineHeight);
            int index = m_textStyle.GetCursorStringIndex(textRect, content, pos);

            var cpos = m_textStyle.GetCursorPixelPosition(textRect, content, index);
            m_selectXOff = cpos.x;
            m_selectCol = index;

        }

        private void EvtKeyDown(KeyCode code,Event e)
        {
            DataCheck();

            switch(code)
            {
                case KeyCode.UpArrow:
                    CMDLineUp();
                    return;
                case KeyCode.DownArrow:
                    CMDLineDown();
                    return;
                case KeyCode.LeftArrow:
                    CMDLineLeft();
                    return;
                case KeyCode.RightArrow:
                    CMDLineRight();
                    return;
                case KeyCode.Backspace:
                    CMDDeleteChar();
                    return;
                case KeyCode.Return:
                    CMDLineReturn();
                    return;
                case KeyCode.KeypadEnter:
                    CMDLineReturn();
                    return;
                case KeyCode.LeftShift:

                    return;
                case KeyCode.RightShift:

                    return;
                case KeyCode.LeftControl:

                    return;
                case KeyCode.RightControl:

                    return;
                case KeyCode.Space:
                    CMDInsertString(m_selectLine, m_selectCol, " ");
                    return;
                case KeyCode.Tab:
                    CMDInsertString(m_selectLine, m_selectCol, "\t");
                    return;

            }

            if ((int)code >= 97 && (int)code <= 122)
            {
                string addc = e.shift ? code.ToString() : code.ToString().ToLower();
                CMDInsertString(m_selectLine, m_selectCol, addc);
            }
            else
            {

                var dic = e.shift ? KEYMAP_SHIFT : KEYMAP;
                if(dic.ContainsKey(code))
                    CMDInsertString(m_selectLine, m_selectCol, dic[code]);
            }
        }

        private void EventScrollWheel(Event e)
        {
            int scrollline = Mathf.RoundToInt(e.delta.y);
            m_render_startLine += scrollline;
            m_render_startLine = m_render_startLine < 0 ? 0 : m_render_startLine;

            if(m_render_startLine > m_data.Count - m_line)
            {
                m_render_startLine = m_data.Count - m_line;
            }


        }

        #endregion

        private Rect GetTextRect() { return new Rect(m_lineCodeOff, m_selectLine * m_lineHeight, m_mainRect.width, m_lineHeight); }
        private void RefreshTextColPosition()
        {
            if (m_selectLine < 0) m_selectLine = 0;

            if (m_selectCol > m_data[m_selectLine].Length)
                m_selectCol = m_data[m_selectLine].Length;

            var cpos = m_textStyle.GetCursorPixelPosition(GetTextRect(), new GUIContent(m_data[m_selectLine]), m_selectCol);
            m_selectXOff = cpos.x;
        }

        private void CMDDeleteChar()
        {
            string curline = m_data[m_selectLine];
            if (curline.Length ==0)
            {
                CMDRemoveLine(m_selectLine);

                m_selectLine--;
                m_selectLine = m_selectLine < 0 ? 0 : m_selectLine;
                CMDSetPosToLineEnd();

                RefreshTextColPosition();
            }
            else
            {
                if(m_selectCol == 0)
                {
                    if (m_selectLine == 0) return;
                    m_selectCol = CMDGetLineLength(m_selectLine-1);
                    m_data[m_selectLine - 1] += m_data[m_selectLine];
                    CMDRemoveLine(m_selectLine);

                    m_selectLine--;
                    RefreshTextColPosition();
                }
                else
                {
                    string linestr = m_data[m_selectLine];
                    m_data[m_selectLine] = linestr.Remove(m_selectCol-1, 1);
                    m_selectCol--;
                    RefreshTextColPosition();
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
        private void CMDInsertString(int line,int col,string c)
        {
            m_data[line] = m_data[m_selectLine].Insert(col, c);
            m_selectCol+= c.Length;
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
        private void CMDLineReturn()
        {
            string curline = m_data[m_selectLine];
            string head = curline.Substring(0, m_selectCol);
            string tail = curline.Substring(m_selectCol);

            m_data[m_selectLine] = head;
            m_selectLine++;
            m_selectCol = 0;
            CMDInsertLine(m_selectLine);
            m_data[m_selectLine] = tail;

            RefreshTextColPosition();
        }
        private int CMDGetLineLength(int line)
        {
            return m_data[line].Length;
        }
        private void CMDSetPosToLineEnd()
        {
            if(m_data.Count >m_selectLine)
            {
                m_selectCol = m_data[m_selectLine].Length;
            }

        }
        private void CMDInsertLine(int line)
        {
            m_data.Insert(line, "");
        }
        private void CMDRemoveLine(int line)
        {
            m_data.RemoveAt(line);
        }
        public void CMDSave()
        {
            Debug.Log("save");
        }
        public void CMDOpenFile(string path = null)
        {
            if(!string.IsNullOrEmpty(path))
                m_filePath = path;

            Debug.Log(m_filePath);
            if (string.IsNullOrEmpty(m_filePath)) return;


            string fileFullpath = Application.dataPath.Substring(0, Application.dataPath.Length - 6) + m_filePath;
            Debug.Log(fileFullpath);
            m_data = new List<string>(File.ReadAllLines(fileFullpath));


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
                if ((m_render_startLine + i) >= m_data.Count) return;

                //line bg
                if (i % 2 == 1) EditorGUI.DrawRect(lineRect, m_colBgLine);

                //line code
                GUI.Label(lineCodeRect, (m_render_startLine+ i + 1).ToString());

                //text
                GUI.Label(lineRect, m_data[m_render_startLine+i], m_textStyle);

                lineRect.y += m_lineHeight;
                lineCodeRect.y += m_lineHeight;


            }
        }
        private void DrawCursor()
        {
            if (!m_cursorShow) return;

            m_cursorPos.x = m_selectXOff;
            m_cursorPos.y = m_lineHeight * (m_selectLine - m_render_startLine);
            m_cursorPos.width = 2f;
            m_cursorPos.height = m_lineHeight;


            EditorGUI.DrawRect(m_cursorPos, Color.white);
        }
        private void DrawInfo()
        {
            EditorGUI.DrawRect(m_infoRect, m_colInfo);
            GUI.Label(m_infoRect, m_infoContext);
        }

        private void DrawScroller()
        {
            if (m_data.Count <= m_line) return;

            EditorGUI.DrawRect(m_scrollerRect, Color.black);

            float percent = m_line * 1.0f / m_data.Count;

            m_scrollerHandleRect.height = m_scrollerRect.height * percent;
            m_scrollerHandleRect.y = m_render_startLine * 1.0f / m_data.Count * m_scrollerRect.height;

            EditorGUI.DrawRect(m_scrollerHandleRect, m_colLineCode);

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

