
ALTER FUNCTION FN_TODATETIME(
	-- Add the parameters for the function here
	 @DATE as date
	,@TIME as int)
RETURNS DateTime
AS
BEGIN
	
	DECLARE	 @OUT	as datetime
			,@TIMEF as nvarchar(8)


	SET @TIMEF =	CASE LEN(@TIME)
						WHEN 1 THEN '00:00:00'
						WHEN 3 THEN CONVERT(VARCHAR(15), Stuff(@TIME, 2, 0, ':'), 4) + ':00'
						WHEN 4 THEN CONVERT(VARCHAR(15), Stuff(@TIME, 3, 0, ':'), 4) + ':00'
						WHEN 5 THEN CONVERT(VARCHAR(15), Stuff(Stuff(@TIME, 2, 0, ':'), 5, 0, ':'), 8)
						WHEN 6 THEN CONVERT(VARCHAR(15), Stuff(Stuff(@TIME, 3, 0, ':'), 6, 0, ':'), 8) END

	SET @OUT = CONVERT(DATETIME, CONVERT(nvarchar(10), @DATE, 121) +  + ' ' + @TIMEF, 121)
	-- Return the result of the function
	RETURN @OUT

END
GO

SELECT cardcode,CreateDate,UpdateDate, UpdateTS, CreateTS,  dbo.FN_TODATETIME(ISNULL(UpdateDate,CreateDate)
		,ISNULL(UpdateTS,CreateTS)) as DateTime FROM OCRD