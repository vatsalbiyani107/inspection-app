USE [JSL_HSM_DB]
GO
/****** Object:  Trigger [dbo].[TRG_Pike_FillFixedValues]    Script Date: 2/26/2026 11:46:35 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER TRIGGER [dbo].[TRG_Pike_FillFixedValues]
ON [dbo].[SLAB_LMS_DETAILS_PikeStoresData]
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Only act if Grade or PlateMode could be relevant
    IF NOT (UPDATE(STEELGRADE) OR UPDATE(PlateMode) OR UPDATE(SLABID))
        RETURN;

    UPDATE p
       SET
           p.Series                       = f.Series,
           p.RoughStripThickness          = f.RoughStripThickness,
           p.FinishedStripThickness       = f.FinishedStripThickness,
           p.FinishedStripAltThickness    = f.FinishedStripAltThickness,
           p.FinishedStripProfile         = f.FinishedStripProfile,
           p.FinishedStripDilatometerCurve= f.FinishedStripDilatometerCurve
    FROM dbo.SLAB_LMS_DETAILS_PikeStoresData p
    INNER JOIN inserted i
        ON i.SLABID = p.SLABID
    LEFT JOIN dbo.Grade_PlateMode_FixedValues f
        ON LTRIM(RTRIM(f.Grade)) = LTRIM(RTRIM(i.STEELGRADE))
       AND f.PlateMode = i.PlateMode;


END;
