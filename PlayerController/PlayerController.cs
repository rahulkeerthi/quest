﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest;
using System.Xml;

namespace AxeSoftware.Quest
{
    public class PlayerController
    {
        private IASL m_game;
        private string m_filename;

        private string m_font = "";
        private string m_fontSize = "";
        private string m_foreground = "";
        private string m_foregroundOverride = "";
        private string m_linkForeground = "";
        private bool m_useForeground = true;
        private string m_fontSizeOverride = "";
        private string m_textBuffer = "";

        public event Action<string> SetAlignment;
        public event Action<string> OutputText;
        public event Action<string, string, string> BindMenu;
        public event Action<string> LocationUpdated;
        public event Action<string> GameNameUpdated;
        public event Action ClearScreen;
        public event Action<string> ShowPicture;
        public event Action<bool> SetPanesVisible;
        public event Action<string> SetStatusText;
        public event Action<string> SetBackground;
        public event Action<string> RunScript;
        public event Action Finished;

        public PlayerController(string filename, string libraryFolder)
        {
            m_filename = filename;

            switch (System.IO.Path.GetExtension(m_filename).ToLower())
            {
                case ".aslx":
                    m_game = new WorldModel(m_filename, libraryFolder);
                    break;
                case ".asl":
                case ".cas":
                    m_game = new AxeSoftware.Quest.LegacyASL.LegacyGame(m_filename);
                    break;
                default:
                    throw new Exception("Unrecognised file type");
            }

            m_game.PrintText += m_game_PrintText;
            m_game.RequestRaised += m_game_RequestRaised;

            // TO DO: All IASL and IASLTimer Events should be handled here
            //m_game.LogError += m_game_LogError;
            //m_game.UpdateList += m_game_UpdateList;
            //m_game.Finished += m_game_Finished;

            //m_gameTimer = m_game as IASLTimer;
            //if (m_gameTimer != null)
            //{
            //    m_gameTimer.UpdateTimer += m_gameTimer_UpdateTimer;
            //}
        }

        public bool Initialise(out List<string> errors, IPlayer player)
        {
            m_game.Initialise(player);
            if (m_game.Errors.Count > 0)
            {
                errors = m_game.Errors;
                return false;
            }
            else
            {
                errors = null;
                return true;
            }
        }

        void m_game_PrintText(string text)
        {
            string currentCommand = "";
            string currentTagValue = "";
            string currentVerbs = "";
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = false;
            XmlReader reader = XmlReader.Create(new System.IO.StringReader(text), settings);
            bool nobr = false;
            bool alignmentSet = false;

            // TO DO: Throw an exception if an <a> tag tries to embed another <a> tag.
            // Should also apply for many of the other tags too.

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.Name)
                        {
                            case "output":
                                if (reader.GetAttribute("nobr") == "true")
                                {
                                    nobr = true;
                                }
                                break;
                            case "object":
                                currentCommand = "look at";
                                currentVerbs = reader.GetAttribute("verbs");
                                break;
                            case "exit":
                                currentCommand = "go";
                                break;
                            case "br":
                                WriteText(FormatText("<br />"));
                                break;
                            case "b":
                                WriteText("<b>");
                                break;
                            case "i":
                                WriteText("<i>");
                                break;
                            case "u":
                                WriteText("<u>");
                                break;
                            case "color":
                                m_foregroundOverride = reader.GetAttribute("color");
                                break;
                            case "font":
                                m_fontSizeOverride = reader.GetAttribute("size");
                                break;
                            case "align":
                                SetAlignment(reader.GetAttribute("align"));
                                break;
                            case "a":
                                m_useForeground = false;
                                string target = reader.GetAttribute("target");
                                WriteText(string.Format("<a href=\"{0}\"{1}>",
                                    reader.GetAttribute("href"),
                                    target != null ? "target=\"" + target + "\"" : ""));
                                break;
                            default:
                                throw new Exception(String.Format("Unrecognised element '{0}'", reader.Name));
                        }
                        break;
                    case XmlNodeType.Text:
                        if (currentCommand.Length == 0)
                        {
                            WriteText(FormatText(reader.Value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;")));
                        }
                        else
                        {
                            currentTagValue = reader.Value;
                        }
                        break;
                    case XmlNodeType.Whitespace:
                        WriteText(FormatText(reader.Value.Replace(" ", "&nbsp;")));
                        break;
                    case XmlNodeType.EndElement:
                        switch (reader.Name)
                        {
                            case "output":
                                // do nothing
                                break;
                            case "object":
                                AddLink(currentTagValue, currentCommand + " " + currentTagValue, currentVerbs);
                                currentCommand = "";
                                break;
                            case "exit":
                                AddLink(currentTagValue, currentCommand + " " + currentTagValue, null);
                                currentCommand = "";
                                break;
                            case "b":
                                WriteText("</b>");
                                break;
                            case "i":
                                WriteText("</i>");
                                break;
                            case "u":
                                WriteText("</u>");
                                break;
                            case "color":
                                m_foregroundOverride = "";
                                break;
                            case "font":
                                m_fontSizeOverride = "";
                                break;
                            case "align":
                                SetAlignment("");
                                alignmentSet = true;
                                break;
                            case "a":
                                WriteText("</a>");
                                m_useForeground = true;
                                break;
                            default:
                                throw new Exception(String.Format("Unrecognised element '{0}'", reader.Name));
                        }
                        break;
                }
            }

            if (!nobr)
            {
                // If we have just set the text alignment but then print no more text afterwards,
                // there's no need to submit an extra <br> tag as subsequent text will be in a
                // brand new <div> element.

                if (!(alignmentSet && m_textBuffer.Length == 0))
                {
                    WriteText("<br />");
                }
            }
        }

        void m_game_RequestRaised(Request request, string data)
        {
            //Logging.Log.DebugFormat("{0} Request raised: {1}, {2}", GameId, request, data);

            switch (request)
            {
                case Request.UpdateLocation:
                    LocationUpdated(data);
                    break;
                case Request.FontName:
                    m_font = data;
                    break;
                case Request.FontSize:
                    m_fontSize = data;
                    break;
                case Request.GameName:
                    GameNameUpdated(data);
                    break;
                case Request.ClearScreen:
                    OutputText(ClearBuffer());
                    ClearScreen();
                    break;
                case Request.ShowPicture:
                    ShowPicture(data);
                    break;
                case Request.PanesVisible:
                    SetPanesVisible(data == "on");
                    break;
                case Request.SetStatus:
                    SetStatusText(data);
                    break;
                case Request.Speak:
                    // ignore
                    break;
                case Request.Foreground:
                    m_foreground = data;
                    break;
                case Request.Background:
                    SetBackground(data);
                    break;
                case Request.RunScript:
                    RunScript(data);
                    break;
                case Request.LinkForeground:
                    m_linkForeground = data;
                    break;
                case Request.Load:
                case Request.Save:
                    OutputText("Sorry, loading and saving is not currently supported for online games. <a href=\"http://www.axeuk.com/quest/\">Download Quest</a> for load/save support.");
                    break;
                case Request.Restart:
                    OutputText("Sorry, restarting is not currently supported for online games. Refresh your browser to restart the game.");
                    break;
                case Request.Quit:
                    Finished();
                    break;
                default:
                    OutputText(string.Format("Unhandled request: {0}, {1}", request, data));
                    break;
            }
        }

        private string FormatText(string text)
        {
            string style = "";
            if (m_font.Length > 0)
            {
                style += string.Format("font-family:{0};", m_font);
            }

            if (m_useForeground)
            {
                string colour = m_foregroundOverride;
                if (colour.Length == 0) colour = m_foreground;
                if (colour.Length > 0)
                {
                    style += string.Format("color:{0};", colour);
                }
            }

            string fontSize = m_fontSizeOverride;
            if (fontSize.Length == 0) fontSize = m_fontSize;

            if (fontSize.Length > 0)
            {
                style += string.Format("font-size:{0}pt;", fontSize);
            }

            if (style.Length > 0)
            {
                text = string.Format("<span style=\"{0}\">{1}</span>", style, text);
            }

            return text;
        }

        private int m_linkCount = 0;

        private void AddLink(string text, string command, string verbs)
        {
            string onclick = string.Empty;
            m_linkCount++;
            string linkid = "verbLink" + m_linkCount.ToString();

            if (string.IsNullOrEmpty(verbs))
            {
                onclick = string.Format(" onclick=\"sendCommand('{0}')\"", command);
            }

            m_useForeground = false;

            // TO DO: I think we should be calling FormatText on the "text" variable below,
            // but currently if we do this we end up with <a...><span>text</span></a> which
            // for some reason breaks jjmenu, meaning we don't see menus when we left-click.
            // The whole area of formatting needs looking at again anyway as it's wasteful to
            // have loads of repeated <span> elements, and this class needs refactoring anyway...

            WriteText(string.Format("<a id=\"{3}\" class=\"cmdlink\"{0}{1}>{2}</a>",
                ((m_linkForeground.Length > 0) ? (" style=color:" + m_linkForeground) + " " : ""),
                onclick,
                text,
                linkid));
            m_useForeground = true;

            // We need to call the JavaScript that binds the pop-up menu to the link *after* it has been
            // written. So, clear the text buffer, then add the binding.
            if (!string.IsNullOrEmpty(verbs))
            {
                OutputText(ClearBuffer());
                BindMenu(linkid, verbs, text);
            }
        }

        private void WriteText(string text)
        {
            m_textBuffer += text;
        }

        public string ClearBuffer()
        {
            string output = m_textBuffer;
            m_textBuffer = "";
            return output;
        }

        // TO DO: This is temporary just while we refactor. Eventually both Player and WebPlayer should
        // have no reference to IASL
        public IASL Game
        {
            get { return m_game; }
        }

    }
}
