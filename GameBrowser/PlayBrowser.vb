﻿Friend Class PlayBrowser
    Private m_recentItems As RecentItems
    Private WithEvents m_onlineGames As New OnlineGames

    Public Event LaunchGame(filename As String)

    Public Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        m_recentItems = New RecentItems("Recent")
        ctlGameList.LaunchCaption = "Play"
        ctlOnlineGameList.LaunchCaption = "Play"
        Populate()
    End Sub

    Public Sub AddToRecent(filename As String, name As String)
        m_recentItems.AddToRecent(filename, name)
    End Sub

    Private Sub ctlGameList_Launch(filename As String) Handles ctlGameList.Launch
        RaiseEvent LaunchGame(filename)
    End Sub

    Private Sub ctlOnlineGameList_Launch(filename As String) Handles ctlOnlineGameList.Launch
        RaiseEvent LaunchGame(filename)
    End Sub

    Public Sub Populate()
        m_recentItems.PopulateGameList(ctlGameList)
    End Sub

    Public Sub MainWindowShown()
        m_onlineGames.StartDownloadGameData()
    End Sub

    Private Sub m_onlineGames_DataReady() Handles m_onlineGames.DataReady
        BeginInvoke(Sub() PopulateCategories())
    End Sub

    Private Sub PopulateCategories()
        ctlBrowseFilter.Populate((From cat In m_onlineGames.Categories Select cat.Title).ToArray())
    End Sub

    Private Sub ctlBrowseFilter_CategoryChanged(category As String) Handles ctlBrowseFilter.CategoryChanged
        PopulateGames(category)
    End Sub

    Private Sub PopulateGames(category As String)
        m_onlineGames.PopulateGameList(category, ctlOnlineGameList)
    End Sub

End Class
