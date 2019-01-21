sp_configure 'show advanced options', 1;  
GO  
RECONFIGURE;  
GO  
sp_configure 'Ole Automation Procedures', 1;  
GO  
RECONFIGURE;  
GO  

IF NOT object_id('dbo.RegExReplace') IS NULL
DROP FUNCTION dbo.RegExReplace 
GO
CREATE FUNCTION dbo.RegExReplace
(
	@string VARCHAR(8000),
	@REGEXP VARCHAR(500),
	@replacestr VARCHAR(8000),
	@casesensitive bit = 0
)
RETURNS VARCHAR(8000) AS 
BEGIN
	DECLARE @handle INT, @RESULT VARCHAR(8000)
 
	EXEC sp_oacreate 'vbscript.regexp', @handle output
	EXEC sp_oasetproperty @handle, 'pattern', @REGEXP
	EXEC sp_oasetproperty @handle, 'global', 'true'
	EXEC sp_oasetproperty @handle, 'ignorecase', @casesensitive
	EXEC sp_oamethod @handle, 'replace', @RESULT output, @string, @replacestr
	EXEC sp_oadestroy @handle
 
	RETURN @RESULT
END
GO
print dbo.RegExReplace('Bruno sda da13 434 !!!!!|\', '[^0-9a-zA-Z]+', '', DEFAULT)