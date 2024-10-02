USE [XposHealtDEV]
GO

SELECT	[Id]
		,[FriendlyName]
		,[Value] FROM Parameters
	WHERE [FriendlyName] in (
		'SatelliteAgents'
		,'DownloadAgents'
		,'StorageConnString'
		,'SizePart'
	)