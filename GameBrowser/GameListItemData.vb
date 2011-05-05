﻿Friend Class GameListItemData
    Private m_filename As String
    Private m_name As String
    Private m_url As String
    Private m_downloadFilename As String

    Public Sub New(filename As String, gameName As String)
        m_filename = filename
        m_name = gameName
    End Sub

    Public Sub New(gameName As String, url As String, downloadFilename As String)
        m_name = gameName
        m_url = url
        m_downloadFilename = downloadFilename
    End Sub

    Public Property Filename() As String
        Set(value As String)
            m_filename = value
        End Set
        Get
            Return m_filename
        End Get
    End Property

    Public ReadOnly Property GameName() As String
        Get
            Return m_name
        End Get
    End Property

    Public ReadOnly Property URL As String
        Get
            Return m_url
        End Get
    End Property

    Public ReadOnly Property DownloadFilename As String
        Get
            Return m_downloadFilename
        End Get
    End Property
End Class