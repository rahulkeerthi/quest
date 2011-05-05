﻿Imports Microsoft.Win32

Friend Class GameList
    Private m_launchCaption As String
    Private m_gameListItems As New List(Of GameListItem)

    Public Event Launch(filename As String)

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        ' This gets around a bug where the appearance of the vertical scrollbar causes contained controls not to resize
        Dim p As Padding = ctlTableLayout.Padding
        ctlTableLayout.Padding = New Padding(p.Left, p.Top, SystemInformation.VerticalScrollBarWidth + p.Right, p.Bottom)
    End Sub

    Public Property LaunchCaption() As String
        Get
            Return m_launchCaption
        End Get
        Set(value As String)
            m_launchCaption = value
        End Set
    End Property

    Public Sub CreateListElements(list As List(Of GameListItemData))
        Dim newItem As GameListItem
        Dim count As Integer = 0

        ClearListElements()

        For Each data As GameListItemData In list
            count += 1

            newItem = New GameListItem
            newItem.Dock = DockStyle.Fill
            newItem.Width = Me.Width
            newItem.GameName = data.GameName
            If String.IsNullOrEmpty(data.Filename) Then
                newItem.URL = data.URL

                Dim downloadFolder As String = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "Quest Games",
                    "Downloaded Games")

                System.IO.Directory.CreateDirectory(downloadFolder)

                Dim downloadFilename As String = System.IO.Path.Combine(
                    downloadFolder, data.DownloadFilename)

                newItem.DownloadFilename = downloadFilename

                If System.IO.File.Exists(downloadFilename) Then
                    ' The file has already been downloaded
                    data.Filename = downloadFilename
                End If
            End If

            If Not String.IsNullOrEmpty(data.Filename) Then
                newItem.GameInfo = System.IO.Path.GetFileName(data.Filename)
                newItem.Filename = data.Filename
                newItem.LaunchCaption = m_launchCaption
            Else
                newItem.CurrentState = GameListItem.State.NotDownloaded
            End If

            AddHandler newItem.Launch, AddressOf LaunchHandler
            m_gameListItems.Add(newItem)

            ctlTableLayout.Controls.Add(newItem)
        Next
    End Sub

    Private Sub ClearListElements()
        For Each item In m_gameListItems
            RemoveHandler item.Launch, AddressOf LaunchHandler
            ctlTableLayout.Controls.Remove(item)
        Next
        m_gameListItems.Clear()
    End Sub

    Private Sub LaunchHandler(filename As String)
        RaiseEvent Launch(filename)
    End Sub
End Class

