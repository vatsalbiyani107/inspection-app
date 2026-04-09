USE [JSL_HSM_DB]
GO
/****** Object:  StoredProcedure [dbo].[Insert_Slab_Master]    Script Date: 4/9/2026 12:57:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[Insert_Slab_Master]
(
    @UploadBatchID VARCHAR(12),
    @SlabId        VARCHAR(12),
    @PlateMode     VARCHAR(10),
    @FurnaceNo     INT,
    @ColdSlabFlag  VARCHAR(10),
    @Code          INT OUTPUT,
    @Msg           VARCHAR(200) OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;
    SET @Code = 500;
    SET @Msg  = 'Unknown error';

    -- ✅ PlateMode conversion
    DECLARE @PlateModeInt INT;
    SET @PlateModeInt =
        CASE UPPER(LTRIM(RTRIM(@PlateMode)))
            WHEN 'LP' THEN 1
            WHEN 'HP' THEN 2
            ELSE 0
        END;

    -- ✅ FurnaceNo validation
    SET @FurnaceNo = CASE
        WHEN LTRIM(RTRIM(@FurnaceNo)) = '2' THEN 2
        ELSE 1
    END;

    -- ✅ ColdSlabFlag conversion
    DECLARE @ColdSlabFlagInt INT;
    SET @ColdSlabFlagInt =
        CASE UPPER(LTRIM(RTRIM(@ColdSlabFlag)))
            WHEN 'HOT'  THEN 1
            WHEN 'COLD' THEN 0
            ELSE 0
        END;

    -- ✅ Validation: SlabId must be exactly 11 characters
    IF LEN(LTRIM(RTRIM(@SlabId))) <> 11
    BEGIN
        SET @Code = 400;
        SET @Msg  = 'SlabId must be exactly 11 characters';
        RETURN;
    END

    -- ✅ Duplicate check in active slabs table
    IF EXISTS (SELECT 1 FROM dbo.Slab_Upload_Masters WHERE SlabID = @SlabId)
    BEGIN
        SET @Code = 409;
        SET @Msg  = 'SlabId already exists in active slabs';
        RETURN;
    END

    -- ✅ Duplicate check in history table
    IF EXISTS (SELECT 1 FROM dbo.PIKE_HISTORY WHERE SlabID = @SlabId)
    BEGIN
        SET @Code = 409;
        SET @Msg  = 'SlabId already exists in history';
        RETURN;
    END

    -- ✅ Step 1: Local insert inside transaction
    BEGIN TRY
        BEGIN TRANSACTION;
            INSERT INTO dbo.Slab_Upload_Masters
            (UploadBatchID, Upload_Date, SlabID, PlateMode, FurnaceNo, ColdSlabFlag, Status)
            VALUES
            (@UploadBatchID, SYSDATETIME(), @SlabId, @PlateModeInt, @FurnaceNo, @ColdSlabFlagInt, 0);
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        SET @Code = 500;
        SET @Msg  = ERROR_MESSAGE();
        RETURN;
    END CATCH
