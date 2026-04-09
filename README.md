USE [JSL_HSM_DB]
GO
/****** Object:  StoredProcedure [dbo].[Insert_Slab_Master]    Script Date: 4/9/2026 2:42:58 PM ******/
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

			INSERT INTO [dbo].[Slab_Upload_Master_L2]
			(SlabID, Upload_Date, Status)
			values
			(@SlabId, SYSDATETIME(), 1);
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        SET @Code = 500;
        SET @Msg  = ERROR_MESSAGE();
        RETURN;
    END CATCH

    -- ✅ Step 2: Remote insert OUTSIDE transaction
    -- Local is already committed — remote failure will not affect it
   /* BEGIN TRY
        IF NOT EXISTS (
            SELECT 1 FROM [10.7.82.180].[HSM_LMS_DB].[dbo].[Slab_Upload_Master] WITH (NOLOCK)
            WHERE SlabID = @SlabId
        )
        BEGIN
            INSERT INTO [10.7.82.180].[HSM_LMS_DB].[dbo].[Slab_Upload_Master] 
            (SlabID, Upload_Date, Status)
            VALUES (@SlabId, GETDATE(), 1);
        END
    END TRY
    BEGIN CATCH
        -- Remote insert failed — local is already saved safely
        -- Optionally log: INSERT INTO dbo.SyncErrors (SlabID, ErrorMsg) VALUES (@SlabId, ERROR_MESSAGE())
    END CATCH*/

    SET @Code = 200;
    SET @Msg  = 'Slab inserted successfully';
END


----------------------------------------------------------------------------------------------------------------------


USE [JSL_HSM_DB]
GO
/****** Object:  Trigger [dbo].[TRG_L2_To_LMS_Details]    Script Date: 4/9/2026 2:43:47 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER   TRIGGER [dbo].[TRG_L2_To_LMS_Details] ON [dbo].[SLAB_LMS_DETAILS_L2] 
AFTER INSERT AS 
BEGIN 
SET NOCOUNT ON;

-- ── Step 1: Insert into SLAB_LMS_DETAILS ─────────────────────────────────
-- Conditions:
--   1. SLABID exists in Slab_Upload_Masters
--   2. Not already in SLAB_LMS_DETAILS
--   3. STATUS = 0 only (STATUS = 1 means already processed, skip)
INSERT INTO [dbo].[SLAB_LMS_DETAILS]
(
    SLABID, STEELGRADE, SLABLENGTH, SLABTHICKNESS, SLABWEIGHT,
    SLABWIDTHHEAD, SLABWIDTHTAIL,
    ANALYSIS_AE, ANALYSIS_AL, ANALYSIS_AS, ANALYSIS_B, ANALYSIS_BE,
    ANALYSIS_BI, ANALYSIS_C, ANALYSIS_CA, ANALYSIS_CE, ANALYSIS_CO,
    ANALYSIS_CR, ANALYSIS_CU, ANALYSIS_H, ANALYSIS_LA, ANALYSIS_MG,
    ANALYSIS_MN, ANALYSIS_MO, ANALYSIS_N, ANALYSIS_NB, ANALYSIS_NI,
    ANALYSIS_O, ANALYSIS_P, ANALYSIS_PB, ANALYSIS_PD, ANALYSIS_S,
    ANALYSIS_SB, ANALYSIS_SE, ANALYSIS_SI, ANALYSIS_SN, ANALYSIS_TA,
    ANALYSIS_TE, ANALYSIS_TI, ANALYSIS_V, ANALYSIS_W, ANALYSIS_ZN,
    ANALYSIS_ZR, CASTERLINE, HEATNO, STATUS, READ_TIME
)
SELECT
    i.SLABID, i.STEELGRADE, i.SLABLENGTH, i.SLABTHICKNESS, i.SLABWEIGHT,
    i.SLABWIDTHHEAD, i.SLABWIDTHTAIL,
    i.ANALYSIS_AE, i.ANALYSIS_AL, i.ANALYSIS_AS, i.ANALYSIS_B, i.ANALYSIS_BE,
    i.ANALYSIS_BI, i.ANALYSIS_C, i.ANALYSIS_CA, i.ANALYSIS_CE, i.ANALYSIS_CO,
    i.ANALYSIS_CR, i.ANALYSIS_CU, i.ANALYSIS_H, i.ANALYSIS_LA, i.ANALYSIS_MG,
    i.ANALYSIS_MN, i.ANALYSIS_MO, i.ANALYSIS_N, i.ANALYSIS_NB, i.ANALYSIS_NI,
    i.ANALYSIS_O, i.ANALYSIS_P, i.ANALYSIS_PB, i.ANALYSIS_PD, i.ANALYSIS_S,
    i.ANALYSIS_SB, i.ANALYSIS_SE, i.ANALYSIS_SI, i.ANALYSIS_SN, i.ANALYSIS_TA,
    i.ANALYSIS_TE, i.ANALYSIS_TI, i.ANALYSIS_V, i.ANALYSIS_W, i.ANALYSIS_ZN,
    i.ANALYSIS_ZR, i.CASTERLINE, i.HEATNO, i.STATUS, i.READ_TIME
FROM inserted i
INNER JOIN dbo.Slab_Upload_Masters um
    ON um.SLABID = i.SLABID
WHERE i.STATUS = 0                          -- skip already processed rows
AND NOT EXISTS (
    SELECT 1 FROM dbo.SLAB_LMS_DETAILS d
    WHERE d.SLABID = i.SLABID
);

-- ── Step 2: Update STATUS = 1 in SLAB_LMS_DETAILS_L2 ────────────────────
-- Only for rows that were matched and inserted
UPDATE l2
SET l2.STATUS = 1
FROM dbo.SLAB_LMS_DETAILS_L2 l2
INNER JOIN inserted i ON i.SLABID = l2.SLABID
INNER JOIN dbo.Slab_Upload_Masters um ON um.SLABID = i.SLABID
WHERE i.STATUS = 0                          -- skip already processed rows
END; 

