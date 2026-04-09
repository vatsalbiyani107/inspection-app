USE [JSL_HSM_DB]
GO

/****** Object:  Table [dbo].[Slab_Upload_Master_L2]    Script Date: 4/9/2026 2:39:22 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Slab_Upload_Master_L2](
	[SLABID] [varchar](12) NOT NULL,
	[Upload_Date] [datetime] NULL,
	[Status] [int] NULL,
 CONSTRAINT [PK_Slab_Upload_Master_L2] PRIMARY KEY CLUSTERED 
(
	[SLABID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO


______________________________________________________________________________________________________________________________



USE [JSL_HSM_DB]
GO

/****** Object:  Table [dbo].[SLAB_LMS_DETAILS_L2]    Script Date: 4/9/2026 2:39:48 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[SLAB_LMS_DETAILS_L2](
	[SLABID] [varchar](12) NOT NULL,
	[STEELGRADE] [varchar](70) NOT NULL,
	[SLABLENGTH] [varchar](70) NULL,
	[SLABTHICKNESS] [varchar](70) NULL,
	[SLABWEIGHT] [varchar](70) NULL,
	[SLABWIDTHHEAD] [varchar](70) NULL,
	[SLABWIDTHTAIL] [varchar](70) NULL,
	[ANALYSIS_AE] [varchar](22) NULL,
	[ANALYSIS_AL] [varchar](22) NULL,
	[ANALYSIS_AS] [varchar](22) NULL,
	[ANALYSIS_B] [varchar](22) NULL,
	[ANALYSIS_BE] [varchar](22) NULL,
	[ANALYSIS_BI] [varchar](22) NULL,
	[ANALYSIS_C] [varchar](22) NULL,
	[ANALYSIS_CA] [varchar](22) NULL,
	[ANALYSIS_CE] [varchar](22) NULL,
	[ANALYSIS_CO] [varchar](22) NULL,
	[ANALYSIS_CR] [varchar](22) NULL,
	[ANALYSIS_CU] [varchar](22) NULL,
	[ANALYSIS_H] [varchar](22) NULL,
	[ANALYSIS_LA] [varchar](22) NULL,
	[ANALYSIS_MG] [varchar](22) NULL,
	[ANALYSIS_MN] [varchar](22) NULL,
	[ANALYSIS_MO] [varchar](22) NULL,
	[ANALYSIS_N] [varchar](22) NULL,
	[ANALYSIS_NB] [varchar](22) NULL,
	[ANALYSIS_NI] [varchar](22) NULL,
	[ANALYSIS_O] [varchar](22) NULL,
	[ANALYSIS_P] [varchar](22) NULL,
	[ANALYSIS_PB] [varchar](22) NULL,
	[ANALYSIS_PD] [varchar](22) NULL,
	[ANALYSIS_S] [varchar](22) NULL,
	[ANALYSIS_SB] [varchar](22) NULL,
	[ANALYSIS_SE] [varchar](22) NULL,
	[ANALYSIS_SI] [varchar](22) NULL,
	[ANALYSIS_SN] [varchar](22) NULL,
	[ANALYSIS_TA] [varchar](22) NULL,
	[ANALYSIS_TE] [varchar](22) NULL,
	[ANALYSIS_TI] [varchar](22) NULL,
	[ANALYSIS_V] [varchar](22) NULL,
	[ANALYSIS_W] [varchar](22) NULL,
	[ANALYSIS_ZN] [varchar](22) NULL,
	[ANALYSIS_ZR] [varchar](22) NULL,
	[CASTERLINE] [varchar](22) NULL,
	[HEATNO] [varchar](70) NULL,
	[STATUS] [int] NOT NULL,
	[READ_TIME] [datetime] NULL,
	[SAP_TIME] [datetime] NULL,
 CONSTRAINT [PK_SLAB_LMS_DETAILS_L2] PRIMARY KEY CLUSTERED 
(
	[SLABID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO



