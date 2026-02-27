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


--------------------------------------------------------------------------------------------------------

USE [JSL_HSM_DB]
GO
/****** Object:  StoredProcedure [dbo].[Get_AllSlabList]    Script Date: 2/27/2026 11:23:35 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[Get_AllSlabList]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        s.*,

        /* =========================================================
           CHEM VALIDATION (ONLY columns existing in Min_Max_ChemData)
        ========================================================= */

        -- C
        CASE
            WHEN TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_C)), '')) IS NULL THEN 0
            WHEN r.MIN_C IS NULL OR r.MAX_C IS NULL THEN 0
            WHEN TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_C)), '')) < r.MIN_C
              OR TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_C)), '')) > r.MAX_C THEN 1
            ELSE 0
        END AS ANALYSIS_C__Fail,
        CASE
            WHEN r.MIN_C IS NULL OR r.MAX_C IS NULL THEN 'Allowed range not defined'
            ELSE CONCAT('C allowed: ', r.MIN_C, ' – ', r.MAX_C)
        END AS ANALYSIS_C__Tip,

        -- MN
        CASE
            WHEN TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_MN)), '')) IS NULL THEN 0
            WHEN r.MIN_MN IS NULL OR r.MAX_MN IS NULL THEN 0
            WHEN TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_MN)), '')) < r.MIN_MN
              OR TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_MN)), '')) > r.MAX_MN THEN 1
            ELSE 0
        END AS ANALYSIS_MN__Fail,
        CASE
            WHEN r.MIN_MN IS NULL OR r.MAX_MN IS NULL THEN 'Allowed range not defined'
            ELSE CONCAT('Mn allowed: ', r.MIN_MN, ' – ', r.MAX_MN)
        END AS ANALYSIS_MN__Tip,

        -- S
        CASE
            WHEN TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_S)), '')) IS NULL THEN 0
            WHEN r.MIN_S IS NULL OR r.MAX_S IS NULL THEN 0
            WHEN TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_S)), '')) < r.MIN_S
              OR TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_S)), '')) > r.MAX_S THEN 1
            ELSE 0
        END AS ANALYSIS_S__Fail,
        CASE
            WHEN r.MIN_S IS NULL OR r.MAX_S IS NULL THEN 'Allowed range not defined'
            ELSE CONCAT('S allowed: ', r.MIN_S, ' – ', r.MAX_S)
        END AS ANALYSIS_S__Tip,

        -- P
        CASE
            WHEN TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_P)), '')) IS NULL THEN 0
            WHEN r.MIN_P IS NULL OR r.MAX_P IS NULL THEN 0
            WHEN TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_P)), '')) < r.MIN_P
              OR TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_P)), '')) > r.MAX_P THEN 1
            ELSE 0
        END AS ANALYSIS_P__Fail,
        CASE
            WHEN r.MIN_P IS NULL OR r.MAX_P IS NULL THEN 'Allowed range not defined'
            ELSE CONCAT('P allowed: ', r.MIN_P, ' – ', r.MAX_P)
        END AS ANALYSIS_P__Tip,

        -- SI
        CASE
            WHEN TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_SI)), '')) IS NULL THEN 0
            WHEN r.MIN_SI IS NULL OR r.MAX_SI IS NULL THEN 0
            WHEN TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_SI)), '')) < r.MIN_SI
              OR TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_SI)), '')) > r.MAX_SI THEN 1
            ELSE 0
        END AS ANALYSIS_SI__Fail,
        CASE
            WHEN r.MIN_SI IS NULL OR r.MAX_SI IS NULL THEN 'Allowed range not defined'
            ELSE CONCAT('Si allowed: ', r.MIN_SI, ' – ', r.MAX_SI)
        END AS ANALYSIS_SI__Tip,

        -- NI
        CASE
            WHEN TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_NI)), '')) IS NULL THEN 0
            WHEN r.MIN_NI IS NULL OR r.MAX_NI IS NULL THEN 0
            WHEN TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_NI)), '')) < r.MIN_NI
              OR TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_NI)), '')) > r.MAX_NI THEN 1
            ELSE 0
        END AS ANALYSIS_NI__Fail,
        CASE
            WHEN r.MIN_NI IS NULL OR r.MAX_NI IS NULL THEN 'Allowed range not defined'
            ELSE CONCAT('Ni allowed: ', r.MIN_NI, ' – ', r.MAX_NI)
        END AS ANALYSIS_NI__Tip,

        -- CR
        CASE
            WHEN TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_CR)), '')) IS NULL THEN 0
            WHEN r.MIN_CR IS NULL OR r.MAX_CR IS NULL THEN 0
            WHEN TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_CR)), '')) < r.MIN_CR
              OR TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_CR)), '')) > r.MAX_CR THEN 1
            ELSE 0
        END AS ANALYSIS_CR__Fail,
        CASE
            WHEN r.MIN_CR IS NULL OR r.MAX_CR IS NULL THEN 'Allowed range not defined'
            ELSE CONCAT('Cr allowed: ', r.MIN_CR, ' – ', r.MAX_CR)
        END AS ANALYSIS_CR__Tip,

        -- CU
        CASE
            WHEN TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_CU)), '')) IS NULL THEN 0
            WHEN r.MIN_CU IS NULL OR r.MAX_CU IS NULL THEN 0
            WHEN TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_CU)), '')) < r.MIN_CU
              OR TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_CU)), '')) > r.MAX_CU THEN 1
            ELSE 0
        END AS ANALYSIS_CU__Fail,
        CASE
            WHEN r.MIN_CU IS NULL OR r.MAX_CU IS NULL THEN 'Allowed range not defined'
            ELSE CONCAT('Cu allowed: ', r.MIN_CU, ' – ', r.MAX_CU)
        END AS ANALYSIS_CU__Tip,

        -- MO
        CASE
            WHEN TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_MO)), '')) IS NULL THEN 0
            WHEN r.MIN_MO IS NULL OR r.MAX_MO IS NULL THEN 0
            WHEN TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_MO)), '')) < r.MIN_MO
              OR TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_MO)), '')) > r.MAX_MO THEN 1
            ELSE 0
        END AS ANALYSIS_MO__Fail,
        CASE
            WHEN r.MIN_MO IS NULL OR r.MAX_MO IS NULL THEN 'Allowed range not defined'
            ELSE CONCAT('Mo allowed: ', r.MIN_MO, ' – ', r.MAX_MO)
        END AS ANALYSIS_MO__Tip,

        -- TI
        CASE
            WHEN TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_TI)), '')) IS NULL THEN 0
            WHEN r.MIN_TI IS NULL OR r.MAX_TI IS NULL THEN 0
            WHEN TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_TI)), '')) < r.MIN_TI
              OR TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_TI)), '')) > r.MAX_TI THEN 1
            ELSE 0
        END AS ANALYSIS_TI__Fail,
        CASE
            WHEN r.MIN_TI IS NULL OR r.MAX_TI IS NULL THEN 'Allowed range not defined'
            ELSE CONCAT('Ti allowed: ', r.MIN_TI, ' – ', r.MAX_TI)
        END AS ANALYSIS_TI__Tip,

        -- V
        CASE
            WHEN TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_V)), '')) IS NULL THEN 0
            WHEN r.MIN_V IS NULL OR r.MAX_V IS NULL THEN 0
            WHEN TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_V)), '')) < r.MIN_V
              OR TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_V)), '')) > r.MAX_V THEN 1
            ELSE 0
        END AS ANALYSIS_V__Fail,
        CASE
            WHEN r.MIN_V IS NULL OR r.MAX_V IS NULL THEN 'Allowed range not defined'
            ELSE CONCAT('V allowed: ', r.MIN_V, ' – ', r.MAX_V)
        END AS ANALYSIS_V__Tip,

        -- H
        CASE
            WHEN TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_H)), '')) IS NULL THEN 0
            WHEN r.MIN_H IS NULL OR r.MAX_H IS NULL THEN 0
            WHEN TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_H)), '')) < r.MIN_H
              OR TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_H)), '')) > r.MAX_H THEN 1
            ELSE 0
        END AS ANALYSIS_H__Fail,
        CASE
            WHEN r.MIN_H IS NULL OR r.MAX_H IS NULL THEN 'Allowed range not defined'
            ELSE CONCAT('H allowed: ', r.MIN_H, ' – ', r.MAX_H)
        END AS ANALYSIS_H__Tip,

        -- N
        CASE
            WHEN TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_N)), '')) IS NULL THEN 0
            WHEN r.MIN_N IS NULL OR r.MAX_N IS NULL THEN 0
            WHEN TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_N)), '')) < r.MIN_N
              OR TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_N)), '')) > r.MAX_N THEN 1
            ELSE 0
        END AS ANALYSIS_N__Fail,
        CASE
            WHEN r.MIN_N IS NULL OR r.MAX_N IS NULL THEN 'Allowed range not defined'
            ELSE CONCAT('N allowed: ', r.MIN_N, ' – ', r.MAX_N)
        END AS ANALYSIS_N__Tip,

        -- PB
        CASE
            WHEN TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_PB)), '')) IS NULL THEN 0
            WHEN r.MIN_PB IS NULL OR r.MAX_PB IS NULL THEN 0
            WHEN TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_PB)), '')) < r.MIN_PB
              OR TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_PB)), '')) > r.MAX_PB THEN 1
            ELSE 0
        END AS ANALYSIS_PB__Fail,
        CASE
            WHEN r.MIN_PB IS NULL OR r.MAX_PB IS NULL THEN 'Allowed range not defined'
            ELSE CONCAT('Pb allowed: ', r.MIN_PB, ' – ', r.MAX_PB)
        END AS ANALYSIS_PB__Tip,

        -- SN
        CASE
            WHEN TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_SN)), '')) IS NULL THEN 0
            WHEN r.MIN_SN IS NULL OR r.MAX_SN IS NULL THEN 0
            WHEN TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_SN)), '')) < r.MIN_SN
              OR TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_SN)), '')) > r.MAX_SN THEN 1
            ELSE 0
        END AS ANALYSIS_SN__Fail,
        CASE
            WHEN r.MIN_SN IS NULL OR r.MAX_SN IS NULL THEN 'Allowed range not defined'
            ELSE CONCAT('Sn allowed: ', r.MIN_SN, ' – ', r.MAX_SN)
        END AS ANALYSIS_SN__Tip,

        -- AL
        CASE
            WHEN TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_AL)), '')) IS NULL THEN 0
            WHEN r.MIN_AL IS NULL OR r.MAX_AL IS NULL THEN 0
            WHEN TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_AL)), '')) < r.MIN_AL
              OR TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_AL)), '')) > r.MAX_AL THEN 1
            ELSE 0
        END AS ANALYSIS_AL__Fail,
        CASE
            WHEN r.MIN_AL IS NULL OR r.MAX_AL IS NULL THEN 'Allowed range not defined'
            ELSE CONCAT('Al allowed: ', r.MIN_AL, ' – ', r.MAX_AL)
        END AS ANALYSIS_AL__Tip,

        -- CA
        CASE
            WHEN TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_CA)), '')) IS NULL THEN 0
            WHEN r.MIN_CA IS NULL OR r.MAX_CA IS NULL THEN 0
            WHEN TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_CA)), '')) < r.MIN_CA
              OR TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_CA)), '')) > r.MAX_CA THEN 1
            ELSE 0
        END AS ANALYSIS_CA__Fail,
        CASE
            WHEN r.MIN_CA IS NULL OR r.MAX_CA IS NULL THEN 'Allowed range not defined'
            ELSE CONCAT('Ca allowed: ', r.MIN_CA, ' – ', r.MAX_CA)
        END AS ANALYSIS_CA__Tip,

        -- B
        CASE
            WHEN TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_B)), '')) IS NULL THEN 0
            WHEN r.MIN_B IS NULL OR r.MAX_B IS NULL THEN 0
            WHEN TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_B)), '')) < r.MIN_B
              OR TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_B)), '')) > r.MAX_B THEN 1
            ELSE 0
        END AS ANALYSIS_B__Fail,
        CASE
            WHEN r.MIN_B IS NULL OR r.MAX_B IS NULL THEN 'Allowed range not defined'
            ELSE CONCAT('B allowed: ', r.MIN_B, ' – ', r.MAX_B)
        END AS ANALYSIS_B__Tip,

        -- CO
        CASE
            WHEN TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_CO)), '')) IS NULL THEN 0
            WHEN r.MIN_CO IS NULL OR r.MAX_CO IS NULL THEN 0
            WHEN TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_CO)), '')) < r.MIN_CO
              OR TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_CO)), '')) > r.MAX_CO THEN 1
            ELSE 0
        END AS ANALYSIS_CO__Fail,
        CASE
            WHEN r.MIN_CO IS NULL OR r.MAX_CO IS NULL THEN 'Allowed range not defined'
            ELSE CONCAT('Co allowed: ', r.MIN_CO, ' – ', r.MAX_CO)
        END AS ANALYSIS_CO__Tip,

        -- NB
        CASE
            WHEN TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_NB)), '')) IS NULL THEN 0
            WHEN r.MIN_NB IS NULL OR r.MAX_NB IS NULL THEN 0
            WHEN TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_NB)), '')) < r.MIN_NB
              OR TRY_CONVERT(DECIMAL(18,6), NULLIF(LTRIM(RTRIM(s.ANALYSIS_NB)), '')) > r.MAX_NB THEN 1
            ELSE 0
        END AS ANALYSIS_NB__Fail,
        CASE
            WHEN r.MIN_NB IS NULL OR r.MAX_NB IS NULL THEN 'Allowed range not defined'
            ELSE CONCAT('Nb allowed: ', r.MIN_NB, ' – ', r.MAX_NB)
        END AS ANALYSIS_NB__Tip,

        /* =========================================================
           DIM VALIDATION (JOIN ON Grade + PlateMode ONLY)
        ========================================================= */

        -- Thickness
        CASE
            WHEN TRY_CONVERT(DECIMAL(18,3), NULLIF(LTRIM(RTRIM(s.SLABTHICKNESS)), '')) IS NULL THEN 0
            WHEN d.ThicknessMinMM IS NULL OR d.ThicknessMaxMM IS NULL THEN 0
            WHEN TRY_CONVERT(DECIMAL(18,3), NULLIF(LTRIM(RTRIM(s.SLABTHICKNESS)), '')) < d.ThicknessMinMM
              OR TRY_CONVERT(DECIMAL(18,3), NULLIF(LTRIM(RTRIM(s.SLABTHICKNESS)), '')) > d.ThicknessMaxMM THEN 1
            ELSE 0
        END AS SLABTHICKNESS__Fail,
        CASE
            WHEN d.ThicknessMinMM IS NULL OR d.ThicknessMaxMM IS NULL THEN 'Allowed range not defined'
            ELSE CONCAT('Thickness allowed: ', d.ThicknessMinMM, ' – ', d.ThicknessMaxMM, ' mm (PlateMode ', s.PlateMode, ')')
        END AS SLABTHICKNESS__Tip,

        -- Length
        CASE
            WHEN TRY_CONVERT(DECIMAL(18,3), NULLIF(LTRIM(RTRIM(s.SLABLENGTH)), '')) IS NULL THEN 0
            WHEN d.LengthMinM IS NULL OR d.LengthMaxM IS NULL THEN 0
            WHEN TRY_CONVERT(DECIMAL(18,3), NULLIF(LTRIM(RTRIM(s.SLABLENGTH)), '')) < d.LengthMinM
              OR TRY_CONVERT(DECIMAL(18,3), NULLIF(LTRIM(RTRIM(s.SLABLENGTH)), '')) > d.LengthMaxM THEN 1
            ELSE 0
        END AS SLABLENGTH__Fail,
        CASE
            WHEN d.LengthMinM IS NULL OR d.LengthMaxM IS NULL THEN 'Allowed range not defined'
            ELSE CONCAT('Length allowed: ', d.LengthMinM, ' – ', d.LengthMaxM, ' m (PlateMode ', s.PlateMode, ')')
        END AS SLABLENGTH__Tip,

        -- Head Width
        CASE
            WHEN TRY_CONVERT(DECIMAL(18,3), NULLIF(LTRIM(RTRIM(s.SLABWIDTHHEAD)), '')) IS NULL THEN 0
            WHEN d.HeadWidthMinMM IS NULL OR d.HeadWidthMaxMM IS NULL THEN 0
            WHEN TRY_CONVERT(DECIMAL(18,3), NULLIF(LTRIM(RTRIM(s.SLABWIDTHHEAD)), '')) < d.HeadWidthMinMM
              OR TRY_CONVERT(DECIMAL(18,3), NULLIF(LTRIM(RTRIM(s.SLABWIDTHHEAD)), '')) > d.HeadWidthMaxMM THEN 1
            ELSE 0
        END AS SLABWIDTHHEAD__Fail,
        CASE
            WHEN d.HeadWidthMinMM IS NULL OR d.HeadWidthMaxMM IS NULL THEN 'Allowed range not defined'
            ELSE CONCAT('Head width allowed: ', d.HeadWidthMinMM, ' – ', d.HeadWidthMaxMM, ' mm (PlateMode ', s.PlateMode, ')')
        END AS SLABWIDTHHEAD__Tip,

        -- Tail Width
        CASE
            WHEN TRY_CONVERT(DECIMAL(18,3), NULLIF(LTRIM(RTRIM(s.SLABWIDTHTAIL)), '')) IS NULL THEN 0
            WHEN d.TailWidthMinMM IS NULL OR d.TailWidthMaxMM IS NULL THEN 0
            WHEN TRY_CONVERT(DECIMAL(18,3), NULLIF(LTRIM(RTRIM(s.SLABWIDTHTAIL)), '')) < d.TailWidthMinMM
              OR TRY_CONVERT(DECIMAL(18,3), NULLIF(LTRIM(RTRIM(s.SLABWIDTHTAIL)), '')) > d.TailWidthMaxMM THEN 1
            ELSE 0
        END AS SLABWIDTHTAIL__Fail,
        CASE
            WHEN d.TailWidthMinMM IS NULL OR d.TailWidthMaxMM IS NULL THEN 'Allowed range not defined'
            ELSE CONCAT('Tail width allowed: ', d.TailWidthMinMM, ' – ', d.TailWidthMaxMM, ' mm (PlateMode ', s.PlateMode, ')')
        END AS SLABWIDTHTAIL__Tip,

        -- Temperature tooltip (if your dim table has Temperature)
        CASE
            WHEN d.Temperature IS NULL THEN NULL
            ELSE CONCAT('Target temperature: ', d.Temperature)
        END AS Temperature__Tip

    FROM dbo.SLAB_LMS_DETAILS_PikeStoresData s

    /* ---- Chem rules join: Grade ---- */
    LEFT JOIN dbo.Min_Max_ChemData r
      ON LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(r.Grade, CHAR(9), ''), CHAR(13), ''), CHAR(10), '')))
       = LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(s.STEELGRADE, CHAR(9), ''), CHAR(13), ''), CHAR(10), '')))

    /* ---- Dim rules join: Grade + PlateMode (0/1/2) ---- */
    LEFT JOIN dbo.GradeWise_ValidateValueWith_Min_Max d
      ON LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(d.Grade, CHAR(9), ''), CHAR(13), ''), CHAR(10), '')))
       = LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(s.STEELGRADE, CHAR(9), ''), CHAR(13), ''), CHAR(10), '')))
     AND TRY_CONVERT(INT, NULLIF(LTRIM(RTRIM(CONVERT(VARCHAR(50), d.PlateMode))), ''))
       = TRY_CONVERT(INT, NULLIF(LTRIM(RTRIM(CONVERT(VARCHAR(50), s.PlateMode))), ''))

    ORDER BY s.SLABID;
END

----------------------------------------------------------------------
USE [JSL_HSM_DB]
GO
/****** Object:  StoredProcedure [dbo].[usp_PrepareOraclePushTable]    Script Date: 2/27/2026 11:36:48 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[usp_PrepareOraclePushTable]
(
    @UploadBatchID NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    ------------------------------------------------------------
    -- Step 1: Load Pike rows for this batch + apply grade mapping
    --         (SAP grade kept separately for audit)
    ------------------------------------------------------------
    IF OBJECT_ID('tempdb..#Src') IS NOT NULL DROP TABLE #Src;

    SELECT
        -- Keys
        s.SLABID AS SlabID,

        -- SAP grade (original) + mapped grade to send to L2
       -- s.STEELGRADE AS SteelGrade_SAP,
        COALESCE(m.Level2Grade, s.STEELGRADE) AS GradeToSend,

        -- Dimensions / header
        CAST(s.SLABLENGTH    AS DECIMAL(18,3)) AS SlabLength,
        CAST(s.SLABTHICKNESS AS DECIMAL(18,3)) AS SlabThickness,
        CAST(s.SLABWEIGHT    AS DECIMAL(18,3)) AS SlabWeight,
        CAST(s.SLABWIDTHHEAD AS DECIMAL(18,3)) AS SlabWidthHead,
        CAST(s.SLABWIDTHTAIL AS DECIMAL(18,3)) AS SlabWidthTail,

        s.HEATNO     AS HeatNo,
        s.CASTERLINE AS CasterLine,
        s.PlateMode  AS PlateMode,
        s.FurnaceNo  AS FurnaceNo,
        s.ColdSlabFlag AS ColdSlabFlag,

        CAST(s.READ_TIME AS DATETIME2(0)) AS ReadTime,

        -- Chemistry (wide)
        CAST(s.ANALYSIS_AE AS DECIMAL(18,6)) AS ANALYSIS_AE,
        CAST(s.ANALYSIS_AL AS DECIMAL(18,6)) AS ANALYSIS_AL,
        CAST(s.ANALYSIS_AS AS DECIMAL(18,6)) AS ANALYSIS_AS,
        CAST(s.ANALYSIS_B  AS DECIMAL(18,6)) AS ANALYSIS_B,
        CAST(s.ANALYSIS_BE AS DECIMAL(18,6)) AS ANALYSIS_BE,
        CAST(s.ANALYSIS_BI AS DECIMAL(18,6)) AS ANALYSIS_BI,
        CAST(s.ANALYSIS_C  AS DECIMAL(18,6)) AS ANALYSIS_C,
        CAST(s.ANALYSIS_CA AS DECIMAL(18,6)) AS ANALYSIS_CA,
        CAST(s.ANALYSIS_CE AS DECIMAL(18,6)) AS ANALYSIS_CE,
        CAST(s.ANALYSIS_CO AS DECIMAL(18,6)) AS ANALYSIS_CO,
        CAST(s.ANALYSIS_CR AS DECIMAL(18,6)) AS ANALYSIS_CR,
        CAST(s.ANALYSIS_CU AS DECIMAL(18,6)) AS ANALYSIS_CU,
        CAST(s.ANALYSIS_H  AS DECIMAL(18,6)) AS ANALYSIS_H,
        CAST(s.ANALYSIS_LA AS DECIMAL(18,6)) AS ANALYSIS_LA,
        CAST(s.ANALYSIS_MG AS DECIMAL(18,6)) AS ANALYSIS_MG,
        CAST(s.ANALYSIS_MN AS DECIMAL(18,6)) AS ANALYSIS_MN,
        CAST(s.ANALYSIS_MO AS DECIMAL(18,6)) AS ANALYSIS_MO,
        CAST(s.ANALYSIS_N  AS DECIMAL(18,6)) AS ANALYSIS_N,
        CAST(s.ANALYSIS_NB AS DECIMAL(18,6)) AS ANALYSIS_NB,
        CAST(s.ANALYSIS_NI AS DECIMAL(18,6)) AS ANALYSIS_NI,
        CAST(s.ANALYSIS_O  AS DECIMAL(18,6)) AS ANALYSIS_O,
        CAST(s.ANALYSIS_P  AS DECIMAL(18,6)) AS ANALYSIS_P,
        CAST(s.ANALYSIS_PB AS DECIMAL(18,6)) AS ANALYSIS_PB,
        CAST(s.ANALYSIS_PD AS DECIMAL(18,6)) AS ANALYSIS_PD,
        CAST(s.ANALYSIS_S  AS DECIMAL(18,6)) AS ANALYSIS_S,
        CAST(s.ANALYSIS_SB AS DECIMAL(18,6)) AS ANALYSIS_SB,
        CAST(s.ANALYSIS_SE AS DECIMAL(18,6)) AS ANALYSIS_SE,
        CAST(s.ANALYSIS_SI AS DECIMAL(18,6)) AS ANALYSIS_SI,
        CAST(s.ANALYSIS_SN AS DECIMAL(18,6)) AS ANALYSIS_SN,
        CAST(s.ANALYSIS_TA AS DECIMAL(18,6)) AS ANALYSIS_TA,
        CAST(s.ANALYSIS_TE AS DECIMAL(18,6)) AS ANALYSIS_TE,
        CAST(s.ANALYSIS_TI AS DECIMAL(18,6)) AS ANALYSIS_TI,
        CAST(s.ANALYSIS_V  AS DECIMAL(18,6)) AS ANALYSIS_V,
        CAST(s.ANALYSIS_W  AS DECIMAL(18,6)) AS ANALYSIS_W,
        CAST(s.ANALYSIS_ZN AS DECIMAL(18,6)) AS ANALYSIS_ZN,
        CAST(s.ANALYSIS_ZR AS DECIMAL(18,6)) AS ANALYSIS_ZR

    INTO #Src
    FROM dbo.SLAB_LMS_DETAILS_PikeStoresData s
    LEFT JOIN dbo.SAP_Data_Mapping m
        ON m.SAPGrade = s.STEELGRADE
    WHERE s.UploadBatchID = @UploadBatchID;

    ------------------------------------------------------------
    -- Step 2: If no rows exist for the batch, stop cleanly
    ------------------------------------------------------------
    IF NOT EXISTS (SELECT 1 FROM #Src)
    BEGIN
        SELECT 0 AS RowsPrepared, 'No rows found for this batch' AS Message;
        RETURN;
    END

    ------------------------------------------------------------
    -- Step 3: Safety de-duplication (keep latest ReadTime)
    ------------------------------------------------------------
    IF OBJECT_ID('tempdb..#Final') IS NOT NULL DROP TABLE #Final;

    ;WITH d AS
    (
        SELECT *,
               ROW_NUMBER() OVER (PARTITION BY SlabID ORDER BY ReadTime DESC) AS rn
        FROM #Src
    )
    SELECT *
    INTO #Final
    FROM d
    WHERE rn = 1;

    ------------------------------------------------------------
    -- Step 4: UPSERT into OraclePush_SlabL2
    ------------------------------------------------------------
    MERGE dbo.OraclePush_SlabL2 AS tgt
    USING #Final AS src
      ON tgt.SlabID = src.SlabID
    WHEN MATCHED THEN
      UPDATE SET
        tgt.GradeToSend    = src.GradeToSend,

        tgt.SlabLength     = src.SlabLength,
        tgt.SlabThickness  = src.SlabThickness,
        tgt.SlabWeight     = src.SlabWeight,
        tgt.SlabWidthHead  = src.SlabWidthHead,
        tgt.SlabWidthTail  = src.SlabWidthTail,

        tgt.HeatNo         = src.HeatNo,
        tgt.CasterLine     = src.CasterLine,
        tgt.PlateMode      = src.PlateMode,
        tgt.FurnaceNo      = src.FurnaceNo,
        tgt.ColdSlabFlag   = src.ColdSlabFlag,

        tgt.ReadTime       = src.ReadTime,

        -- Chemistry
        tgt.ANALYSIS_AE = src.ANALYSIS_AE,
        tgt.ANALYSIS_AL = src.ANALYSIS_AL,
        tgt.ANALYSIS_AS = src.ANALYSIS_AS,
        tgt.ANALYSIS_B  = src.ANALYSIS_B,
        tgt.ANALYSIS_BE = src.ANALYSIS_BE,
        tgt.ANALYSIS_BI = src.ANALYSIS_BI,
        tgt.ANALYSIS_C  = src.ANALYSIS_C,
        tgt.ANALYSIS_CA = src.ANALYSIS_CA,
        tgt.ANALYSIS_CE = src.ANALYSIS_CE,
        tgt.ANALYSIS_CO = src.ANALYSIS_CO,
        tgt.ANALYSIS_CR = src.ANALYSIS_CR,
        tgt.ANALYSIS_CU = src.ANALYSIS_CU,
        tgt.ANALYSIS_H  = src.ANALYSIS_H,
        tgt.ANALYSIS_LA = src.ANALYSIS_LA,
        tgt.ANALYSIS_MG = src.ANALYSIS_MG,
        tgt.ANALYSIS_MN = src.ANALYSIS_MN,
        tgt.ANALYSIS_MO = src.ANALYSIS_MO,
        tgt.ANALYSIS_N  = src.ANALYSIS_N,
        tgt.ANALYSIS_NB = src.ANALYSIS_NB,
        tgt.ANALYSIS_NI = src.ANALYSIS_NI,
        tgt.ANALYSIS_O  = src.ANALYSIS_O,
        tgt.ANALYSIS_P  = src.ANALYSIS_P,
        tgt.ANALYSIS_PB = src.ANALYSIS_PB,
        tgt.ANALYSIS_PD = src.ANALYSIS_PD,
        tgt.ANALYSIS_S  = src.ANALYSIS_S,
        tgt.ANALYSIS_SB = src.ANALYSIS_SB,
        tgt.ANALYSIS_SE = src.ANALYSIS_SE,
        tgt.ANALYSIS_SI = src.ANALYSIS_SI,
        tgt.ANALYSIS_SN = src.ANALYSIS_SN,
        tgt.ANALYSIS_TA = src.ANALYSIS_TA,
        tgt.ANALYSIS_TE = src.ANALYSIS_TE,
        tgt.ANALYSIS_TI = src.ANALYSIS_TI,
        tgt.ANALYSIS_V  = src.ANALYSIS_V,
        tgt.ANALYSIS_W  = src.ANALYSIS_W,
        tgt.ANALYSIS_ZN = src.ANALYSIS_ZN,
        tgt.ANALYSIS_ZR = src.ANALYSIS_ZR,

        -- Control fields
        tgt.UploadBatchID = @UploadBatchID,
        tgt.PreparedOn    = SYSDATETIME(),
        tgt.NeedsPush     = 1,
        tgt.PushStatus    = 'PENDING',
        tgt.PushError     = NULL

    WHEN NOT MATCHED THEN
      INSERT
      (
        SlabID, GradeToSend,
        SlabLength, SlabThickness, SlabWeight, SlabWidthHead, SlabWidthTail,
        HeatNo, CasterLine, PlateMode, FurnaceNo, ColdSlabFlag,
        ReadTime,

        ANALYSIS_AE, ANALYSIS_AL, ANALYSIS_AS, ANALYSIS_B, ANALYSIS_BE, ANALYSIS_BI,
        ANALYSIS_C, ANALYSIS_CA, ANALYSIS_CE, ANALYSIS_CO, ANALYSIS_CR, ANALYSIS_CU,
        ANALYSIS_H, ANALYSIS_LA, ANALYSIS_MG, ANALYSIS_MN, ANALYSIS_MO,
        ANALYSIS_N, ANALYSIS_NB, ANALYSIS_NI, ANALYSIS_O,
        ANALYSIS_P, ANALYSIS_PB, ANALYSIS_PD,
        ANALYSIS_S, ANALYSIS_SB, ANALYSIS_SE, ANALYSIS_SI, ANALYSIS_SN,
        ANALYSIS_TA, ANALYSIS_TE, ANALYSIS_TI,
        ANALYSIS_V, ANALYSIS_W, ANALYSIS_ZN, ANALYSIS_ZR,

        UploadBatchID, PreparedOn, NeedsPush, PushStatus
      )
      VALUES
      (
        src.SlabID, src.GradeToSend,
        src.SlabLength, src.SlabThickness, src.SlabWeight, src.SlabWidthHead, src.SlabWidthTail,
        src.HeatNo, src.CasterLine, src.PlateMode, src.FurnaceNo, src.ColdSlabFlag,
        src.ReadTime,

        src.ANALYSIS_AE, src.ANALYSIS_AL, src.ANALYSIS_AS, src.ANALYSIS_B, src.ANALYSIS_BE, src.ANALYSIS_BI,
        src.ANALYSIS_C, src.ANALYSIS_CA, src.ANALYSIS_CE, src.ANALYSIS_CO, src.ANALYSIS_CR, src.ANALYSIS_CU,
        src.ANALYSIS_H, src.ANALYSIS_LA, src.ANALYSIS_MG, src.ANALYSIS_MN, src.ANALYSIS_MO,
        src.ANALYSIS_N, src.ANALYSIS_NB, src.ANALYSIS_NI, src.ANALYSIS_O,
        src.ANALYSIS_P, src.ANALYSIS_PB, src.ANALYSIS_PD,
        src.ANALYSIS_S, src.ANALYSIS_SB, src.ANALYSIS_SE, src.ANALYSIS_SI, src.ANALYSIS_SN,
        src.ANALYSIS_TA, src.ANALYSIS_TE, src.ANALYSIS_TI,
        src.ANALYSIS_V, src.ANALYSIS_W, src.ANALYSIS_ZN, src.ANALYSIS_ZR,

        @UploadBatchID, SYSDATETIME(), 1, 'PENDING'
      );

    ------------------------------------------------------------
    -- Step 5: Return count for UI
    ------------------------------------------------------------
    SELECT COUNT(*) AS RowsPrepared, 'Prepared for Oracle push' AS Message
    FROM #Final;
END

------------------
