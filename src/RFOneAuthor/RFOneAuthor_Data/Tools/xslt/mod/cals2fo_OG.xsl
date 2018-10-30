<?xml version='1.0'?>
<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:fo="http://www.w3.org/1999/XSL/Format"
	xmlns:saxon="http://saxon.sf.net/"
	extension-element-prefixes="saxon" 	>

	<!-- **********************************************************
	tbl.xsl - transforms CALS tables into FOs
customized for SPXDocs US:
	1.  no hyphenation in table heads if language is English
	********************************************************** -->

	<xsl:include href="tbl_layout.xss"/>

	<!-- #########################################-->

	<xsl:template name="copy-string">
		<!-- returns 'count' copies of 'string' -->
		<xsl:param name="string"/>
		<xsl:param name="count" select="0"/>
		<xsl:param name="result"/>

		<xsl:choose>
			<xsl:when test="$count&gt;0">
				<xsl:call-template name="copy-string">
					<xsl:with-param name="string" select="$string"/>
					<xsl:with-param name="count" select="$count - 1"/>
					<xsl:with-param name="result">
						<xsl:value-of select="$result"/>
						<xsl:value-of select="$string"/>
					</xsl:with-param>
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$result"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="blank.spans">
		<xsl:param name="cols" select="1"/>
		<xsl:if test="$cols &gt; 0">
			<xsl:text>0:</xsl:text>
			<xsl:call-template name="blank.spans">
				<xsl:with-param name="cols" select="$cols - 1"/>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template name="calculate.following.spans">
		<xsl:param name="colspan" select="1"/>
		<xsl:param name="spans" select="''"/>

		<xsl:choose>
			<xsl:when test="$colspan &gt; 0">
				<xsl:call-template name="calculate.following.spans">
					<xsl:with-param name="colspan" select="$colspan - 1"/>
					<xsl:with-param name="spans" select="substring-after($spans,':')"/>
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$spans"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="finaltd">
		<xsl:param name="spans"/>
		<xsl:param name="col" select="0"/>

		<xsl:if test="$spans != ''">
			<xsl:choose>
				<xsl:when test="starts-with($spans,'0:')">
					<xsl:call-template name="empty.table.cell">
						<xsl:with-param name="colnum" select="$col"/>
					</xsl:call-template>
				</xsl:when>
				<xsl:otherwise></xsl:otherwise>
			</xsl:choose>

			<xsl:call-template name="finaltd">
				<xsl:with-param name="spans" select="substring-after($spans,':')"/>
				<xsl:with-param name="col" select="$col+1"/>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template name="sfinaltd">
		<xsl:param name="spans"/>

		<xsl:if test="$spans != ''">
			<xsl:choose>
				<xsl:when test="starts-with($spans,'0:')">0:</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="number(substring-before($spans,':'))-1"/>
					<xsl:text>:</xsl:text>
				</xsl:otherwise>
			</xsl:choose>

			<xsl:call-template name="sfinaltd">
				<xsl:with-param name="spans" select="substring-after($spans,':')"/>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template name="entry.colnum">
		<xsl:param name="entry" select="."/>

		<xsl:choose>
			<xsl:when test="$entry/@spanname">
				<xsl:variable name="spanname" select="$entry/@spanname"/>
				<xsl:variable name="spanspec" select="$entry/ancestor::tgroup/spanspec[@spanname=$spanname]"/>
				<xsl:variable name="colspec" select="$entry/ancestor::tgroup/colspec[@colname=$spanspec/@namest]"/>
				<xsl:call-template name="colspec.colnum">
					<xsl:with-param name="colspec" select="$colspec"/>
				</xsl:call-template>
			</xsl:when>
			<xsl:when test="$entry/@colname">
				<xsl:variable name="colname" select="$entry/@colname"/>
				<xsl:variable name="colspec" select="$entry/ancestor::tgroup/colspec[@colname=$colname]"/>
				<xsl:call-template name="colspec.colnum">
					<xsl:with-param name="colspec" select="$colspec"/>
				</xsl:call-template>
			</xsl:when>
			<xsl:when test="$entry/@namest">
				<xsl:variable name="namest" select="$entry/@namest"/>
				<xsl:variable name="colspec" select="$entry/ancestor::tgroup/colspec[@colname=$namest]"/>
				<xsl:call-template name="colspec.colnum">
					<xsl:with-param name="colspec" select="$colspec"/>
				</xsl:call-template>
			</xsl:when>
			<!-- no idea, return 0 -->
			<xsl:otherwise>0</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="colspec.colnum">
		<xsl:param name="colspec" select="."/>
		<xsl:choose>
			<xsl:when test="$colspec/@colnum">
				<xsl:value-of select="$colspec/@colnum"/>
			</xsl:when>
			<xsl:when test="$colspec/preceding-sibling::colspec">
				<xsl:variable name="prec.colspec.colnum">
					<xsl:call-template name="colspec.colnum">
						<xsl:with-param name="colspec"  select="$colspec/preceding-sibling::colspec[1]"/>
					</xsl:call-template>
				</xsl:variable>
				<xsl:value-of select="$prec.colspec.colnum + 1"/>
			</xsl:when>
			<xsl:otherwise>1</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="calculate.colspan">
		<xsl:param name="entry" select="."/>
		<xsl:variable name="spanname" select="$entry/@spanname"/>
		<xsl:variable name="spanspec"
		select="$entry/ancestor::tgroup/spanspec[@spanname=$spanname]"/>

		<xsl:variable name="namest">
			<xsl:choose>
				<xsl:when test="@spanname">
					<xsl:value-of select="$spanspec/@namest"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$entry/@namest"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>

		<xsl:variable name="nameend">
			<xsl:choose>
				<xsl:when test="@spanname">
					<xsl:value-of select="$spanspec/@nameend"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$entry/@nameend"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>

		<xsl:variable name="scol">
			<xsl:call-template name="colspec.colnum">
				<xsl:with-param name="colspec" select="$entry/ancestor::tgroup/colspec[@colname=$namest]"/>
			</xsl:call-template>
		</xsl:variable>

		<xsl:variable name="ecol">
			<xsl:call-template name="colspec.colnum">
				<xsl:with-param name="colspec" select="$entry/ancestor::tgroup/colspec[@colname=$nameend]"/>
			</xsl:call-template>
		</xsl:variable>

		<xsl:choose>
			<xsl:when test="$namest != '' and $nameend != ''">
				<xsl:choose>
					<xsl:when test="$ecol &gt;= $scol">
						<xsl:value-of select="$ecol - $scol + 1"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="$scol - $ecol + 1"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<xsl:otherwise>1</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="calculate.rowsep">
		<xsl:param name="entry" select="."/>
		<xsl:param name="colnum" select="0"/>

		<xsl:call-template name="inherited.table.attribute">
			<xsl:with-param name="entry" select="$entry"/>
			<xsl:with-param name="colnum" select="$colnum"/>
			<xsl:with-param name="attribute" select="'rowsep'"/>
		</xsl:call-template>
	</xsl:template>

	<xsl:template name="calculate.colsep">
		<xsl:param name="entry" select="."/>
		<xsl:param name="colnum" select="0"/>

		<xsl:call-template name="inherited.table.attribute">
			<xsl:with-param name="entry" select="$entry"/>
			<xsl:with-param name="colnum" select="$colnum"/>
			<xsl:with-param name="attribute" select="'colsep'"/>
		</xsl:call-template>
	</xsl:template>

	<xsl:template name="inherited.table.attribute">
		<xsl:param name="entry" select="."/>
		<xsl:param name="row" select="$entry/ancestor-or-self::row[1]"/>
		<xsl:param name="colnum" select="0"/>
		<xsl:param name="attribute" select="'colsep'"/>
		<xsl:param name="lastrow" select="0"/>
		<xsl:param name="lastcol" select="0"/>

		<xsl:variable name="tgroup" select="$row/ancestor::tgroup[1]"/>

		<xsl:variable name="entry.value">
			<xsl:call-template name="get-attribute">
				<xsl:with-param name="element" select="$entry"/>
				<xsl:with-param name="attribute" select="$attribute"/>
			</xsl:call-template>
		</xsl:variable>

		<xsl:variable name="row.value">
			<xsl:call-template name="get-attribute">
				<xsl:with-param name="element" select="$row"/>
				<xsl:with-param name="attribute" select="$attribute"/>
			</xsl:call-template>
		</xsl:variable>

		<xsl:variable name="span.value">
			<xsl:if test="$entry/@spanname">
				<xsl:variable name="spanname" select="$entry/@spanname"/>
				<xsl:variable name="spanspec" select="$tgroup/spanspec[@spanname=$spanname]"/>
				<xsl:variable name="span.colspec" select="$tgroup/colspec[@colname=$spanspec/@namest]"/>
				<xsl:variable name="spanspec.value">
					<xsl:call-template name="get-attribute">
						<xsl:with-param name="element" select="$spanspec"/>
						<xsl:with-param name="attribute" select="$attribute"/>
					</xsl:call-template>
				</xsl:variable>
				<xsl:variable name="scolspec.value">
					<xsl:call-template name="get-attribute">
						<xsl:with-param name="element" select="$span.colspec"/>
						<xsl:with-param name="attribute" select="$attribute"/>
					</xsl:call-template>
				</xsl:variable>
				<xsl:choose>
					<xsl:when test="$spanspec.value != ''">
						<xsl:value-of select="$spanspec.value"/>
					</xsl:when>
					<xsl:when test="$scolspec.value != ''">
						<xsl:value-of select="$scolspec.value"/>
					</xsl:when>
					<xsl:otherwise></xsl:otherwise>
				</xsl:choose>
			</xsl:if>
		</xsl:variable>

		<xsl:variable name="namest.value">
			<xsl:if test="$entry/@namest">
				<xsl:variable name="namest" select="$entry/@namest"/>
				<xsl:variable name="colspec" select="$tgroup/colspec[@colname=$namest]"/>
				<xsl:variable name="namest.value">
					<xsl:call-template name="get-attribute">
						<xsl:with-param name="element" select="$colspec"/>
						<xsl:with-param name="attribute" select="$attribute"/>
					</xsl:call-template>
				</xsl:variable>
				<xsl:choose>
					<xsl:when test="$namest.value">
						<xsl:value-of select="$namest.value"/>
					</xsl:when>
					<xsl:otherwise></xsl:otherwise>
				</xsl:choose>
			</xsl:if>
		</xsl:variable>

		<xsl:variable name="tgroup.value">
			<xsl:choose>
				<!-- Special case to handle thead valign -->
				<xsl:when test="$attribute='valign' and ancestor::thead/@valign">
					<xsl:value-of select="ancestor::thead/@valign"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:call-template name="get-attribute">
						<xsl:with-param name="element" select="$tgroup"/>
						<xsl:with-param name="attribute" select="$attribute"/>
					</xsl:call-template>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>

		<xsl:variable name="default.value">
			<!-- rowsep and colsep can have defaults on the "table" wrapper and
			ultimately on the frame setting for outside rules.  Non-outside
			rules are unaffected by the frame setting.  Both rowsep and colsep
			default to 1 on the table wrapper if otherwise unspecified.  -->
			<!-- handle those here, for everything else, the default is the tgroup value -->
			<xsl:choose>
				<xsl:when test="$tgroup.value != ''">
					<xsl:value-of select="$tgroup.value"/>
				</xsl:when>
				<xsl:when test="$attribute = 'rowsep'">
					<xsl:choose>
						<xsl:when test="$tgroup/parent::*/@rowsep">
							<xsl:value-of select="$tgroup/parent::*/@rowsep"/>
						</xsl:when>
						<xsl:otherwise>1</xsl:otherwise>
					</xsl:choose>
				</xsl:when>
				<xsl:when test="$attribute = 'colsep'">
					<xsl:choose>
						<xsl:when test="$tgroup/parent::*/@colsep">
							<xsl:value-of select="$tgroup/parent::*/@colsep"/>
						</xsl:when>
						<xsl:otherwise>1</xsl:otherwise>
					</xsl:choose>
				</xsl:when>
				<xsl:otherwise>
					<!-- empty -->
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>

		<xsl:variable name="frame.value">
			<xsl:variable name="frame">
				<xsl:choose>
					<xsl:when test="$tgroup/parent::*/@frame">
						<xsl:value-of select="$tgroup/parent::*/@frame"/>
					</xsl:when>
					<xsl:otherwise>all</xsl:otherwise>
				</xsl:choose>
			</xsl:variable>
			<xsl:choose>
				<xsl:when test="$attribute='rowsep'">
					<xsl:choose>
						<xsl:when test="$frame='all' or $frame='topbot' or $frame='bot'">1</xsl:when>
						<xsl:otherwise>0</xsl:otherwise>
					</xsl:choose>
				</xsl:when>
				<xsl:when test="$attribute='colsep'">
					<xsl:choose>
						<xsl:when test="$frame='all' or $frame='sides'">1</xsl:when>
						<xsl:otherwise>0</xsl:otherwise>
					</xsl:choose>
				</xsl:when>
			</xsl:choose>
		</xsl:variable>

		<xsl:choose>
			<xsl:when test="$lastrow=1 and $attribute='rowsep'">
				<xsl:value-of select="$frame.value"/>
			</xsl:when>
			<xsl:when test="$lastcol=1 and $attribute='colsep'">
				<xsl:value-of select="$frame.value"/>
			</xsl:when>
			<xsl:when test="$entry.value != ''">
				<xsl:value-of select="$entry.value"/>
			</xsl:when>
			<xsl:when test="$row.value != ''">
				<xsl:value-of select="$row.value"/>
			</xsl:when>
			<xsl:when test="$span.value != ''">
				<xsl:value-of select="$span.value"/>
			</xsl:when>
			<xsl:when test="$namest.value != ''">
				<xsl:value-of select="$namest.value"/>
			</xsl:when>
			<xsl:when test="$colnum &gt; 0">
				<xsl:variable name="calc.colvalue">
					<xsl:call-template name="colnum.colspec">
						<xsl:with-param name="colnum" select="$colnum"/>
						<xsl:with-param name="attribute" select="$attribute"/>
					</xsl:call-template>
				</xsl:variable>
				<xsl:choose>
					<xsl:when test="$calc.colvalue != ''">
						<xsl:value-of select="$calc.colvalue"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="$default.value"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$default.value"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="colnum.colspec">
		<xsl:param name="colnum" select="0"/>
		<xsl:param name="attribute" select="'colname'"/>
		<xsl:param name="colspecs" select="ancestor::tgroup/colspec"/>
		<xsl:param name="count" select="1"/>

		<xsl:choose>
			<xsl:when test="not($colspecs) or $count &gt; $colnum">
				<!-- nop -->
			</xsl:when>
			<xsl:when test="$colspecs[1]/@colnum">
				<xsl:choose>
					<xsl:when test="$colspecs[1]/@colnum = $colnum">
						<xsl:call-template name="get-attribute">
							<xsl:with-param name="element" select="$colspecs[1]"/>
							<xsl:with-param name="attribute" select="$attribute"/>
						</xsl:call-template>
					</xsl:when>
					<xsl:otherwise>
						<xsl:call-template name="colnum.colspec">
							<xsl:with-param name="colnum" select="$colnum"/>
							<xsl:with-param name="attribute" select="$attribute"/>
							<xsl:with-param name="colspecs" select="$colspecs[position()&gt;1]"/>
							<xsl:with-param name="count" select="$colspecs[1]/@colnum+1"/>
						</xsl:call-template>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<xsl:otherwise>
				<xsl:choose>
					<xsl:when test="$count = $colnum">
						<xsl:call-template name="get-attribute">
							<xsl:with-param name="element" select="$colspecs[1]"/>
							<xsl:with-param name="attribute" select="$attribute"/>
						</xsl:call-template>
					</xsl:when>
					<xsl:otherwise>
						<xsl:call-template name="colnum.colspec">
							<xsl:with-param name="colnum" select="$colnum"/>
							<xsl:with-param name="attribute" select="$attribute"/>
							<xsl:with-param name="colspecs" select="$colspecs[position()&gt;1]"/>
							<xsl:with-param name="count" select="$count+1"/>
						</xsl:call-template>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="get-attribute">
		<xsl:param name="element" select="."/>
		<xsl:param name="attribute" select="''"/>

		<xsl:for-each select="$element/@*">
			<xsl:if test="local-name(.) = $attribute">
				<xsl:value-of select="."/>
			</xsl:if>
		</xsl:for-each>
	</xsl:template>

	<xsl:template name="empty.table.cell">
		<xsl:param name="colnum" select="0"/>

		<xsl:call-template name="setCellColor"/>

		<xsl:variable name="lastrow">
			<xsl:variable name="rows-spanned">
				<xsl:choose>
					<xsl:when test="@morerows">
						<xsl:value-of select="@morerows+1"/>
					</xsl:when>
					<xsl:otherwise>1</xsl:otherwise>
				</xsl:choose>
			</xsl:variable>
			<xsl:choose>
				<xsl:when test="ancestor::thead">0</xsl:when>
				<xsl:when test="ancestor::tfoot and not(ancestor::row/following-sibling::row)">1</xsl:when>
				<xsl:when test="not(ancestor::tfoot) and ancestor::tgroup/tfoot">0</xsl:when>
				<xsl:when test="not(ancestor::tfoot) 	and not(ancestor::tgroup/tfoot) and count(ancestor::row/following-sibling::row) &lt; $rows-spanned">1</xsl:when>
				<xsl:otherwise>0</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>

		<xsl:variable name="lastcol">
			<xsl:variable name="spanname" select="@spanname"/>
			<xsl:variable name="spanspec"
			select="ancestor::tgroup/spanspec[@spanname=$spanname]"/>
			<xsl:variable name="nameend">
				<xsl:choose>
					<xsl:when test="@spanname">
						<xsl:value-of select="$spanspec/@nameend"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="@nameend"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:variable>
			<xsl:variable name="ecol">
				<xsl:choose>
					<xsl:when test="$nameend!=''">
						<xsl:call-template name="colspec.colnum">
							<xsl:with-param name="colspec" select="ancestor::tgroup/colspec[@colname=$nameend]"/>
						</xsl:call-template>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="$colnum"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:variable>
			<xsl:choose>
				<xsl:when test="$ecol &lt; ancestor::tgroup/@cols">0</xsl:when>
				<xsl:otherwise>1</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>

		<xsl:variable name="rowsep">
			<xsl:call-template name="inherited.table.attribute">
				<xsl:with-param name="entry" select="NOT-AN-ELEMENT-NAME"/>
				<xsl:with-param name="row" select="ancestor-or-self::row[1]"/>
				<xsl:with-param name="colnum" select="$colnum"/>
				<xsl:with-param name="attribute" select="'rowsep'"/>
				<xsl:with-param name="lastrow" select="$lastrow"/>
				<xsl:with-param name="lastcol" select="$lastcol"/>
			</xsl:call-template>
		</xsl:variable>

		<xsl:variable name="colsep">
			<xsl:call-template name="inherited.table.attribute">
				<xsl:with-param name="entry" select="NOT-AN-ELEMENT-NAME"/>
				<xsl:with-param name="row" select="ancestor-or-self::row[1]"/>
				<xsl:with-param name="colnum" select="$colnum"/>
				<xsl:with-param name="attribute" select="'colsep'"/>
				<xsl:with-param name="lastrow" select="$lastrow"/>
				<xsl:with-param name="lastcol" select="$lastcol"/>
			</xsl:call-template>
		</xsl:variable>

		<fo:table-cell text-align="center"
			display-align="center"
			xsl:use-attribute-sets="table.cell.padding">
			<xsl:if test="$cellbgcolor != '0'">
				<xsl:attribute name="background-color">
					<xsl:value-of select="$cellbgcolor"/>
				</xsl:attribute>
			</xsl:if>
			<!--

			<xsl:call-template name="maybe-emit-cell-padding-attrs"/>
			<xsl:call-template name="maybe-emit-rtf-direct-attrs"/>
			-->
			<xsl:if test="$rowsep &gt; 0 and $lastrow = 0">
				<xsl:call-template name="border">
					<xsl:with-param name="side" select="'bottom'"/>
				</xsl:call-template>
			</xsl:if>

			<xsl:if test="$colsep &gt; 0 and $lastcol = 0">
				<xsl:call-template name="border">
					<xsl:with-param name="side" select="'right'"/>
				</xsl:call-template>
			</xsl:if>

			<!-- ***** first added call to handle _cellfont ***** -->
			<!--xsl:call-template name="just-after-table-cell-stag"/-->
			<!-- ***** end added line ***** -->

			<!-- fo:table-cell should not be empty -->
			<fo:block/>

			<!-- ***** second added call to handle _cellfont ***** -->
			<!--xsl:call-template name="just-before-table-cell-etag"/-->
			<!-- ***** end added line ***** -->

		</fo:table-cell>
		<saxon:assign name="cellbgcolor" select="'0'"/>
		<saxon:assign name="cellfontcolor" select="'0'"/>

	</xsl:template>

	<!-- ==================================================================== -->

	<xsl:template name="border">
		<xsl:param name="side" select="'left'"/>

		<!-- Maybe set border thickness from PubTbl PI -->
		<xsl:variable name="border-thickness">
			<xsl:choose>
				<xsl:when test="ancestor-or-self::tgroup[1]/processing-instruction('PubTbl')[starts-with(.,'tgroup') and contains(.,' rth=')]">
					<xsl:variable name="rth-pi" 	select="ancestor-or-self::tgroup[1]/processing-instruction('PubTbl')[starts-with(.,'tgroup') and contains(.,' rth=')]"/>
					<xsl:variable name="rth-pi2" select='substring-after($rth-pi," rth=")'/>
					<xsl:value-of select="substring-before(substring($rth-pi2,2),'&quot;')"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$table.border.thickness"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>

		<xsl:attribute name="border-{$side}-width">
			<xsl:value-of select="$border-thickness"/>
		</xsl:attribute>
		<xsl:attribute name="border-{$side}-style">
			<xsl:value-of select="$table.border.style"/>
		</xsl:attribute>
		<xsl:attribute name="border-{$side}-color">
			<xsl:value-of select="$table.border.color"/>
		</xsl:attribute>
		<!-- added to make rows that span pages have closing borders on the bottom -->
		<xsl:attribute name="border-after-width.conditionality" select=" 'retain' "/>
	</xsl:template>

	<!-- ==================================================================== -->

	<!-- This next template is for Epic 4.3 TurboStyler compatibility
	and should be deletable when Styler replaces TurboStyler
	HOWEVER, it is still used by Styler for tables within headers/footers! -->
	<xsl:template match="tgroup">
		<fo:table-and-caption>
			<xsl:choose>
				<xsl:when test="ancestor::table/@align ='center'">
					<xsl:attribute name="text-align">center</xsl:attribute>
				</xsl:when>
				<xsl:when test="ancestor::table/@align ='right'">
					<xsl:attribute name="text-align">right</xsl:attribute>
				</xsl:when>
			</xsl:choose>

			<fo:table xsl:use-attribute-sets="table" table-layout="fixed" >
				<xsl:choose>
					<xsl:when test="parent::table/tnote">
						<xsl:attribute name="space-after">3mm</xsl:attribute>
					</xsl:when>
					<xsl:otherwise>
						<xsl:attribute name="space-after">1em</xsl:attribute>
					</xsl:otherwise>
				</xsl:choose>
				<xsl:if test="ancestor::table/@percentWidth">
					<xsl:attribute name="width">
					<xsl:value-of select="ancestor::table/@percentWidth"/>%</xsl:attribute>
				</xsl:if>
				<xsl:if test="ancestor::table[not(@hyphenation='off')]">
					<xsl:attribute name="hyphenate">true</xsl:attribute>
					<xsl:attribute name="language" select="$language"/>
				</xsl:if>
				<xsl:choose>
					<xsl:when test="count(preceding-sibling::tgroup)=0">
						<xsl:call-template name="tgroup.first"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:call-template name="tgroup.notfirst"/>
					</xsl:otherwise>
				</xsl:choose>
				<!-- default the value of frame to all -->
				<xsl:variable name="frame">
					<xsl:choose>
						<xsl:when test="../@frame">
							<xsl:value-of select="../@frame"/>
						</xsl:when>
						<xsl:otherwise>all</xsl:otherwise>
					</xsl:choose>
				</xsl:variable>
				<xsl:choose>
					<xsl:when test="$frame='all'">
						<xsl:call-template name="border">
							<xsl:with-param name="side" select="'left'"/>
						</xsl:call-template>
						<xsl:call-template name="border">
							<xsl:with-param name="side" select="'right'"/>
						</xsl:call-template>
						<xsl:call-template name="border">
							<xsl:with-param name="side" select="'top'"/>
						</xsl:call-template>
						<xsl:call-template name="border">
							<xsl:with-param name="side" select="'bottom'"/>
						</xsl:call-template>
					</xsl:when>
					<xsl:when test="$frame='bottom'">
						<xsl:call-template name="border">
							<xsl:with-param name="side" select="'bottom'"/>
						</xsl:call-template>
					</xsl:when>
					<xsl:when test="$frame='sides'">
						<xsl:call-template name="border">
							<xsl:with-param name="side" select="'left'"/>
						</xsl:call-template>
						<xsl:call-template name="border">
							<xsl:with-param name="side" select="'right'"/>
						</xsl:call-template>
					</xsl:when>
					<xsl:when test="$frame='top'">
						<xsl:call-template name="border">
							<xsl:with-param name="side" select="'top'"/>
						</xsl:call-template>
					</xsl:when>
					<xsl:when test="$frame='topbot'">
						<xsl:call-template name="border">
							<xsl:with-param name="side" select="'top'"/>
						</xsl:call-template>
						<xsl:call-template name="border">
							<xsl:with-param name="side" select="'bottom'"/>
						</xsl:call-template>
					</xsl:when>
					<xsl:otherwise>
						<!-- frame="none" or invalid -->
					</xsl:otherwise>
				</xsl:choose>
				<xsl:call-template name="tgroup-after-table-fo"/>
			</fo:table>
		</fo:table-and-caption>
	</xsl:template>


	<xsl:template match="tgroup" name="tgroup-after-table-fo" mode="already-emitted-table-fo">
		<xsl:variable name="colspecs">
			<xsl:call-template name="generate.colgroup">
				<xsl:with-param name="cols" select="@cols"/>
			</xsl:call-template>
		</xsl:variable>
		<xsl:copy-of select="$colspecs"/>
		<xsl:apply-templates select="thead"/>
		<xsl:apply-templates select="tfoot"/>
		<xsl:apply-templates select="tbody"/>
	</xsl:template>

	<xsl:template match="colspec"></xsl:template>

	<xsl:template match="spanspec"></xsl:template>

	<xsl:template match="thead">
		<xsl:variable name="tgroup" select="parent::*"/>

		<fo:table-header xsl:use-attribute-sets="table-reset-indents">
			<!-- turn off hyphenation in thead if language = en -->
			<xsl:if test="$language = 'en_US' or $language = 'en_UK' ">
				<xsl:attribute name="hyphenate" select=" 'false' "/>
			</xsl:if>
			<xsl:if test="not(ancestor::table[@colorscheme='manual'])">
				<xsl:call-template name="tbl-head"/>
			</xsl:if>
			<xsl:call-template name="thead"/>
			<xsl:apply-templates select="row[1]">
				<xsl:with-param name="spans">
					<xsl:call-template name="blank.spans">
						<xsl:with-param name="cols" select="../@cols"/>
					</xsl:call-template>
				</xsl:with-param>
			</xsl:apply-templates>
		</fo:table-header>
	</xsl:template>

	<xsl:template match="tfoot">
		<xsl:variable name="tgroup" select="parent::*"/>

		<fo:table-footer xsl:use-attribute-sets="table-reset-indents">
			<xsl:call-template name="tfoot"/>
			<xsl:apply-templates select="row[1]">
				<xsl:with-param name="spans">
					<xsl:call-template name="blank.spans">
						<xsl:with-param name="cols" select="../@cols"/>
					</xsl:call-template>
				</xsl:with-param>
			</xsl:apply-templates>

		</fo:table-footer>
	</xsl:template>

	<xsl:template match="tbody">
		<!-- Added table footnotes as last row in the tbody -->
		<xsl:variable name="tgroup" select="parent::*"/>
		<xsl:variable name="number-columns-spanned" select="count(ancestor::tgroup/colspec)"/>
		<fo:table-body xsl:use-attribute-sets="table-reset-indents">
			<xsl:call-template name="tbody"/>
			<xsl:apply-templates select="row[1]">
				<xsl:with-param name="spans">
					<xsl:call-template name="blank.spans">
						<xsl:with-param name="cols" select="../@cols"/>
					</xsl:call-template>
				</xsl:with-param>
			</xsl:apply-templates>
			<xsl:call-template name="tableFootnotes">
				<xsl:with-param name="number-columns-spanned" select="$number-columns-spanned"/>
			</xsl:call-template>
		</fo:table-body>
	</xsl:template>

	<xsl:template match="row">
		<xsl:param name="spans"/>
		<xsl:variable name="rowPos">
			<xsl:number count="row" from="table" level="any"/>
		</xsl:variable>

		<xsl:choose>
			<xsl:when test="contains($spans, '0')">
				<xsl:call-template name="normal-row">
					<xsl:with-param name="spans" select="$spans"/>
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<!-- <xsl:if test="normalize-space(.//text()) != ''"> <xsl:message>Warning: overlapped row contains content!</xsl:message> </xsl:if> -->
				<fo:table-row>
					<!--	<xsl:if test="not(ancestor::table[@colorscheme='manual'])"> 	<xsl:if test="number($rowPos) mod 2 != 0 and ancestor::tbody"> 	<xsl:attribute name="background-color">#E6E6E6</xsl:attribute> 	</xsl:if> </xsl:if>-->
					<xsl:comment> This row intentionally left blank </xsl:comment>
					<fo:table-cell>
						<fo:block/>
					</fo:table-cell>
				</fo:table-row>

				<xsl:apply-templates select="following-sibling::row[1]">
					<xsl:with-param name="spans">
						<xsl:call-template name="consume-row">
							<xsl:with-param name="spans" select="$spans"/>
						</xsl:call-template>
					</xsl:with-param>
				</xsl:apply-templates>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="consume-row">
		<xsl:param name="spans"/>

		<xsl:if test="contains($spans,':')">
			<xsl:value-of select="number(substring-before($spans,':')) - 1"/>
			<xsl:text>:</xsl:text>
			<xsl:call-template name="consume-row">
				<xsl:with-param name="spans" select="substring-after($spans,':')"/>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template name="normal-row">
		<xsl:param name="spans"/>
		<xsl:variable name="rowPos">
			<xsl:number count="row" from="table" level="any"/>
		</xsl:variable>

		<fo:table-row keep-together.within-page="99">
			<!--	<xsl:if test="not(ancestor::table[@colorscheme='manual'])">
			<xsl:if test="number($rowPos) mod 2 != 0 and ancestor::tbody">
			<xsl:attribute name="background-color">#E6E6E6</xsl:attribute>
			</xsl:if>
		</xsl:if>-->
		<xsl:call-template name="row"/>
		<!-- maybe set the height attribute from a PubTbl row rht value -->
		<!--    <xsl:if test="ancestor-or-self::row[1]/processing-instruction('PubTbl')
		[starts-with(.,'row') and contains(.,' rht=')]">
		<xsl:attribute name="height">
		<xsl:variable name="rht-pi"
		select="ancestor-or-self::row[1]/processing-instruction('PubTbl')
		[starts-with(.,'row') and contains(.,' rht=')]"/>
		<xsl:variable name="rht-pi2" select='substring-after($rht-pi," rht=")'/>
		<xsl:value-of select="substring-before(substring($rht-pi2,2),'&quot;')"/>
		</xsl:attribute>
		</xsl:if>
		-->
		<!-- maybe set the break-before or keep-with-previous attribute
		from a PubTbl row breakpenalty value -->
		<!--    <xsl:if test="ancestor-or-self::row[1]/processing-instruction('PubTbl')
		[starts-with(.,'row') and contains(.,' breakpenalty=')]">
		<xsl:variable name="breakpenalty-pi"
		select="ancestor-or-self::row[1]/processing-instruction('PubTbl')
		[starts-with(.,'row') and contains(.,' breakpenalty=')]"/>
		<xsl:variable name="breakpenalty-pi2" select='substring-after($breakpenalty-pi," breakpenalty=")'/>
		<xsl:variable name="breakpenalty" select="substring-before(substring($breakpenalty-pi2,2),'&quot;')"/>
		<xsl:choose>
		<xsl:when test="$breakpenalty='10000'">
		<xsl:attribute name="keep-with-previous">always</xsl:attribute>
		</xsl:when>
		<xsl:when test="$breakpenalty='-10000'">
		<xsl:attribute name="break-before">column</xsl:attribute>
		</xsl:when>
		</xsl:choose>
		</xsl:if>
		-->
		<xsl:apply-templates select="entry[1]">
			<xsl:with-param name="spans" select="$spans"/>
		</xsl:apply-templates>
	</fo:table-row>

	<xsl:if test="following-sibling::row">
		<xsl:variable name="nextspans">
			<xsl:apply-templates select="entry[1]" mode="span">
				<xsl:with-param name="spans" select="$spans"/>
			</xsl:apply-templates>
		</xsl:variable>

		<xsl:apply-templates select="following-sibling::row[1]">
			<xsl:with-param name="spans" select="$nextspans"/>
		</xsl:apply-templates>
	</xsl:if>
</xsl:template>

<xsl:template match="entry" name="entry-template">
	<xsl:param name="col" select="1"/>
	<xsl:param name="spans"/>

	<xsl:call-template name="setCellColor"/>

	<xsl:variable name="row" select="parent::row"/>
	<xsl:variable name="group" select="$row/parent::*[1]"/>

	<xsl:variable name="empty.cell" select="count(node()) = 0"/>

	<xsl:variable name="named.colnum">
		<xsl:call-template name="entry.colnum"/>
	</xsl:variable>

	<xsl:variable name="entry.colnum">
		<xsl:choose>
			<xsl:when test="$named.colnum &gt; 0">
				<xsl:value-of select="$named.colnum"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$col"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>

	<xsl:variable name="entry.colspan">
		<xsl:choose>
			<xsl:when test="@spanname or @namest">
				<xsl:call-template name="calculate.colspan"/>
			</xsl:when>
			<xsl:otherwise>1</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>

	<xsl:variable name="following.spans">
		<xsl:call-template name="calculate.following.spans">
			<xsl:with-param name="colspan" select="$entry.colspan"/>
			<xsl:with-param name="spans" select="$spans"/>
		</xsl:call-template>
	</xsl:variable>

	<xsl:variable name="lastrow">
		<xsl:variable name="rows-spanned">
			<xsl:choose>
				<xsl:when test="@morerows">
					<xsl:value-of select="@morerows+1"/>
				</xsl:when>
				<xsl:otherwise>1</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:choose>
			<xsl:when test="ancestor::thead">0</xsl:when>
			<xsl:when test="ancestor::tfoot
			and not(ancestor::row/following-sibling::row)">1</xsl:when>
			<xsl:when test="not(ancestor::tfoot)
			and ancestor::tgroup/tfoot">0</xsl:when>
			<xsl:when test="not(ancestor::tfoot) and not(ancestor::tgroup/tfoot)
			and count(ancestor::row/following-sibling::row) &lt; $rows-spanned">1</xsl:when>
			<xsl:otherwise>0</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>

	<xsl:variable name="lastcol">
		<xsl:variable name="spanname" select="@spanname"/>
		<xsl:variable name="spanspec"
		select="ancestor::tgroup/spanspec[@spanname=$spanname]"/>
		<xsl:variable name="nameend">
			<xsl:choose>
				<xsl:when test="@spanname">
					<xsl:value-of select="$spanspec/@nameend"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="@nameend"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="ecol">
			<xsl:choose>
				<xsl:when test="$nameend!=''">
					<xsl:call-template name="colspec.colnum">
						<xsl:with-param name="colspec" 		select="ancestor::tgroup/colspec[@colname=$nameend]"/>
					</xsl:call-template>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$col"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:choose>
			<xsl:when test="$ecol &lt; ancestor::tgroup/@cols">0</xsl:when>
			<xsl:otherwise>1</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>

	<xsl:variable name="rowsep">
		<xsl:call-template name="inherited.table.attribute">
			<xsl:with-param name="entry" select="."/>
			<xsl:with-param name="colnum" select="$entry.colnum"/>
			<xsl:with-param name="attribute" select="'rowsep'"/>
			<xsl:with-param name="lastrow" select="$lastrow"/>
			<xsl:with-param name="lastcol" select="$lastcol"/>
		</xsl:call-template>
	</xsl:variable>

	<!--
	<xsl:message>
	<xsl:value-of select="."/>: <xsl:value-of select="$rowsep"/>
	</xsl:message>
	-->

	<xsl:variable name="colsep">
		<xsl:call-template name="inherited.table.attribute">
			<xsl:with-param name="entry" select="."/>
			<xsl:with-param name="colnum" select="$entry.colnum"/>
			<xsl:with-param name="attribute" select="'colsep'"/>
			<xsl:with-param name="lastrow" select="$lastrow"/>
			<xsl:with-param name="lastcol" select="$lastcol"/>
		</xsl:call-template>
	</xsl:variable>

	<xsl:variable name="valign">
		<xsl:call-template name="inherited.table.attribute">
			<xsl:with-param name="entry" select="."/>
			<xsl:with-param name="colnum" select="$entry.colnum"/>
			<xsl:with-param name="attribute" select="'valign'"/>
		</xsl:call-template>
	</xsl:variable>

	<xsl:variable name="align">
		<xsl:call-template name="inherited.table.attribute">
			<xsl:with-param name="entry" select="."/>
			<xsl:with-param name="colnum" select="$entry.colnum"/>
			<xsl:with-param name="attribute" select="'align'"/>
		</xsl:call-template>
	</xsl:variable>

	<xsl:variable name="char">
		<xsl:call-template name="inherited.table.attribute">
			<xsl:with-param name="entry" select="."/>
			<xsl:with-param name="colnum" select="$entry.colnum"/>
			<xsl:with-param name="attribute" select="'char'"/>
		</xsl:call-template>
	</xsl:variable>

	<xsl:variable name="charoff">
		<xsl:call-template name="inherited.table.attribute">
			<xsl:with-param name="entry" select="."/>
			<xsl:with-param name="colnum" select="$entry.colnum"/>
			<xsl:with-param name="attribute" select="'charoff'"/>
		</xsl:call-template>
	</xsl:variable>

	<xsl:choose>
		<xsl:when test="$spans != '' and not(starts-with($spans,'0:'))">
			<xsl:call-template name="entry-template">
				<xsl:with-param name="col" select="$col+1"/>
				<xsl:with-param name="spans" select="substring-after($spans,':')"/>
			</xsl:call-template>
		</xsl:when>

		<xsl:when test="$entry.colnum &gt; $col">
			<xsl:call-template name="empty.table.cell">
				<xsl:with-param name="colnum" select="$col"/>
			</xsl:call-template>
			<xsl:call-template name="entry-template">
				<xsl:with-param name="col" select="$col+1"/>
				<xsl:with-param name="spans" select="substring-after($spans,':')"/>
			</xsl:call-template>
		</xsl:when>

		<xsl:otherwise>

			<xsl:variable name="cell.content">
				<fo:block>
					<!-- highlight this entry? -->
					<!--xsl:if test="ancestor::thead">
					<xsl:attribute name="font-weight">bold</xsl:attribute>
				</xsl:if-->
				<xsl:if test="$cellfontcolor != '0'">
					<xsl:attribute name="color">
						<xsl:value-of select="$cellfontcolor"/>
					</xsl:attribute>
				</xsl:if>

				<!-- are we missing any indexterms? -->
				<xsl:if test="not(preceding-sibling::entry) 	and not(parent::row/preceding-sibling::row)">
					<!-- this is the first entry of the first row -->
					<xsl:if test="ancestor::thead or 		(ancestor::tbody 		and not(ancestor::tbody/preceding-sibling::thead 		or ancestor::tbody/preceding-sibling::tbody))">
						<!-- of the thead or the first tbody -->
						<xsl:apply-templates select="ancestor::tgroup/preceding-sibling::indexterm"/>
					</xsl:if>
				</xsl:if>
				<!-- <xsl:text>(</xsl:text> <xsl:value-of select="$rowsep"/> <xsl:text>,</xsl:text> <xsl:value-of select="$colsep"/> <xsl:text>)</xsl:text> -->
				<xsl:choose>
					<xsl:when test="$empty.cell">
						<xsl:text>&#160;</xsl:text>
					</xsl:when>
					<xsl:otherwise>
						<xsl:apply-templates/>
					</xsl:otherwise>
				</xsl:choose>
			</fo:block>
		</xsl:variable>

		<fo:table-cell xsl:use-attribute-sets="table.cell.padding">
			<xsl:if test="$cellbgcolor != '0'">
				<xsl:attribute name="background-color">
					<xsl:value-of select="$cellbgcolor"/>
				</xsl:attribute>
			</xsl:if>
			<!--
			<xsl:call-template name="maybe-emit-cell-padding-attrs"/>
			<xsl:call-template name="maybe-emit-rtf-direct-attrs"/>
			-->
			<xsl:if test="$rowsep &gt; 0 and $lastrow = 0">
				<xsl:call-template name="border">
					<xsl:with-param name="side" select="'bottom'"/>
				</xsl:call-template>
			</xsl:if>

			<xsl:if test="$colsep &gt; 0 and $lastcol = 0">
				<xsl:call-template name="border">
					<xsl:with-param name="side" select="'right'"/>
				</xsl:call-template>
			</xsl:if>

			<xsl:if test="@morerows">
				<xsl:attribute name="number-rows-spanned">
					<xsl:value-of select="@morerows+1"/>
				</xsl:attribute>
			</xsl:if>

			<xsl:if test="$entry.colspan &gt; 1">
				<xsl:attribute name="number-columns-spanned">
					<xsl:value-of select="$entry.colspan"/>
				</xsl:attribute>
			</xsl:if>

			<xsl:if test="$valign != ''">
				<xsl:attribute name="display-align">
					<xsl:choose>
						<xsl:when test="$valign='top'">before</xsl:when>
						<xsl:when test="$valign='middle'">center</xsl:when>
						<xsl:when test="$valign='bottom'">after</xsl:when>
						<xsl:otherwise>
							<xsl:message>
								<xsl:text>Unexpected valign value: </xsl:text>
								<xsl:value-of select="$valign"/>
								<xsl:text>, center used.</xsl:text>
							</xsl:message>
							<xsl:text>center</xsl:text>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:attribute>
			</xsl:if>

			<xsl:if test="$align != ''">
				<xsl:attribute name="text-align">
					<xsl:value-of select="$align"/>
				</xsl:attribute>
			</xsl:if>

			<xsl:if test="($char != '') and ($align = 'char')">
				<xsl:attribute name="text-align">
					<xsl:value-of select="$char"/>
				</xsl:attribute>
			</xsl:if>

			<!--
			<xsl:if test="@charoff">
			<xsl:attribute name="charoff">
			<xsl:value-of select="@charoff"/>
			</xsl:attribute>
			</xsl:if>
			-->

			<!-- ***** first added call to handle _cellfont ***** -->
			<!--xsl:call-template name="just-after-table-cell-stag"/-->
			<!-- ***** end added line ***** -->

			<xsl:copy-of select="$cell.content"/>

			<!-- ***** second added call to handle _cellfont ***** -->
			<!--xsl:call-template name="just-before-table-cell-etag"/-->
			<!-- ***** end added line ***** -->

		</fo:table-cell>

		<saxon:assign name="cellbgcolor" select="'0'"/>
		<saxon:assign name="cellfontcolor" select="'0'"/>

		<xsl:choose>
			<xsl:when test="following-sibling::entry">
				<xsl:apply-templates select="following-sibling::entry[1]">
					<xsl:with-param name="col" select="$col+$entry.colspan"/>
					<xsl:with-param name="spans" select="$following.spans"/>
				</xsl:apply-templates>
			</xsl:when>
			<xsl:otherwise>
				<xsl:call-template name="finaltd">
					<xsl:with-param name="spans" select="$following.spans"/>
					<xsl:with-param name="col" select="$col+$entry.colspan"/>
				</xsl:call-template>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:otherwise>
</xsl:choose>
</xsl:template>

<xsl:template name="maybe-emit-cell-padding-attrs">
	<xsl:if test="ancestor-or-self::tgroup[1]/processing-instruction('PubTbl')
		[starts-with(.,'tgroup') and contains(.,'marg=')]">
		<!-- Use PubTbl tgroup values for c{r,l,t,b}marg to set overriding padding-* values -->
		<xsl:variable name="cmarg-pi"
			select="ancestor-or-self::tgroup[1]/processing-instruction('PubTbl')
		[starts-with(.,'tgroup') and contains(.,'marg=')]"/>
		<xsl:if test="contains($cmarg-pi,'crmarg=')">
			<xsl:variable name="marg-pi2" select='substring-after($cmarg-pi,"crmarg=")'/>
			<xsl:attribute name="padding-right">
				<xsl:value-of select="substring-before(substring($marg-pi2,2),'&quot;')"/>
			</xsl:attribute>
		</xsl:if>
		<xsl:if test="contains($cmarg-pi,'clmarg=')">
			<xsl:variable name="marg-pi2" select='substring-after($cmarg-pi,"clmarg=")'/>
			<xsl:attribute name="padding-left">
				<xsl:value-of select="substring-before(substring($marg-pi2,2),'&quot;')"/>
			</xsl:attribute>
		</xsl:if>
		<xsl:if test="contains($cmarg-pi,'ctmarg=')">
			<xsl:variable name="marg-pi2" select='substring-after($cmarg-pi,"ctmarg=")'/>
			<xsl:attribute name="padding-top">
				<xsl:value-of select="substring-before(substring($marg-pi2,2),'&quot;')"/>
			</xsl:attribute>
		</xsl:if>
		<xsl:if test="contains($cmarg-pi,'cbmarg=')">
			<xsl:variable name="marg-pi2" select='substring-after($cmarg-pi,"cbmarg=")'/>
			<xsl:attribute name="padding-bottom">
				<xsl:value-of select="substring-before(substring($marg-pi2,2),'&quot;')"/>
			</xsl:attribute>
		</xsl:if>
	</xsl:if>
</xsl:template>

<xsl:template match="entry" name="sentry" mode="span">
	<xsl:param name="col" select="1"/>
	<xsl:param name="spans"/>

	<xsl:variable name="entry.colnum">
		<xsl:call-template name="entry.colnum"/>
	</xsl:variable>

	<xsl:variable name="entry.colspan">
		<xsl:choose>
			<xsl:when test="@spanname or @namest">
				<xsl:call-template name="calculate.colspan"/>
			</xsl:when>
			<xsl:otherwise>1</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>

	<xsl:variable name="following.spans">
		<xsl:call-template name="calculate.following.spans">
			<xsl:with-param name="colspan" select="$entry.colspan"/>
			<xsl:with-param name="spans" select="$spans"/>
		</xsl:call-template>
	</xsl:variable>

	<xsl:choose>
		<xsl:when test="$spans != '' and not(starts-with($spans,'0:'))">
			<xsl:value-of select="number(substring-before($spans,':'))-1"/>
			<xsl:text>:</xsl:text>
			<xsl:call-template name="sentry">
				<xsl:with-param name="col" select="number($col)+1"/>
				<xsl:with-param name="spans" select="substring-after($spans,':')"/>
			</xsl:call-template>
		</xsl:when>

		<xsl:when test="$entry.colnum &gt; $col">
			<xsl:text>0:</xsl:text>
			<xsl:call-template name="sentry">
				<xsl:with-param name="col" select="$col+$entry.colspan"/>
				<xsl:with-param name="spans" select="$following.spans"/>
			</xsl:call-template>
		</xsl:when>

		<xsl:otherwise>
			<xsl:call-template name="copy-string">
				<xsl:with-param name="count" select="$entry.colspan"/>
				<xsl:with-param name="string">
					<xsl:choose>
						<xsl:when test="@morerows">
							<xsl:value-of select="@morerows"/>
						</xsl:when>
						<xsl:otherwise>0</xsl:otherwise>
					</xsl:choose>
					<xsl:text>:</xsl:text>
				</xsl:with-param>
			</xsl:call-template>

			<xsl:choose>
				<xsl:when test="following-sibling::entry">
					<xsl:apply-templates select="following-sibling::entry[1]" 		mode="span">
						<xsl:with-param name="col" select="$col+$entry.colspan"/>
						<xsl:with-param name="spans" select="$following.spans"/>
					</xsl:apply-templates>
				</xsl:when>
				<xsl:otherwise>
					<xsl:call-template name="sfinaltd">
						<xsl:with-param name="spans" select="$following.spans"/>
					</xsl:call-template>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:otherwise>
	</xsl:choose>
</xsl:template>

<xsl:template name="generate.colgroup.raw">
	<xsl:param name="cols" select="1"/>
	<xsl:param name="count" select="1"/>

	<xsl:choose>
		<xsl:when test="$count>$cols"></xsl:when>
		<xsl:otherwise>
			<xsl:call-template name="generate.col.raw">
				<xsl:with-param name="countcol" select="$count"/>
			</xsl:call-template>
			<xsl:call-template name="generate.colgroup.raw">
				<xsl:with-param name="cols" select="$cols"/>
				<xsl:with-param name="count" select="$count+1"/>
			</xsl:call-template>
		</xsl:otherwise>
	</xsl:choose>
</xsl:template>

<xsl:template name="generate.colgroup">
	<xsl:param name="cols" select="1"/>
	<xsl:param name="count" select="1"/>

	<xsl:choose>
		<xsl:when test="$count>$cols"></xsl:when>
		<xsl:otherwise>
			<xsl:call-template name="generate.col">
				<xsl:with-param name="countcol" select="$count"/>
			</xsl:call-template>
			<xsl:call-template name="generate.colgroup">
				<xsl:with-param name="cols" select="$cols"/>
				<xsl:with-param name="count" select="$count+1"/>
			</xsl:call-template>
		</xsl:otherwise>
	</xsl:choose>
</xsl:template>

<xsl:template name="generate.col.raw">
	<!-- generate the table-column for column countcol -->
	<xsl:param name="countcol">1</xsl:param>
	<xsl:param name="colspecs" select="./colspec"/>
	<xsl:param name="count">1</xsl:param>
	<xsl:param name="colnum">1</xsl:param>

	<xsl:choose>
		<xsl:when test="$count>count($colspecs)">
			<fo:table-column column-number="{$countcol}"/>
		</xsl:when>
		<xsl:otherwise>
			<xsl:variable name="colspec" select="$colspecs[$count=position()]"/>

			<xsl:variable name="colspec.colnum">
				<xsl:choose>
					<xsl:when test="$colspec/@colnum">
						<xsl:value-of select="$colspec/@colnum"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="$colnum"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:variable>

			<xsl:variable name="colspec.colwidth">
				<xsl:choose>
					<xsl:when test="$colspec/@colwidth">
						<xsl:value-of select="$colspec/@colwidth"/>
					</xsl:when>
					<xsl:otherwise>1*</xsl:otherwise>
				</xsl:choose>
			</xsl:variable>

			<xsl:choose>
				<xsl:when test="$colspec.colnum=$countcol">
					<fo:table-column column-number="{$countcol}">
						<xsl:attribute name="column-width">
							<xsl:value-of select="$colspec.colwidth"/>
						</xsl:attribute>
					</fo:table-column>
				</xsl:when>
				<xsl:otherwise>
					<xsl:call-template name="generate.col.raw">
						<xsl:with-param name="countcol" select="$countcol"/>
						<xsl:with-param name="colspecs" select="$colspecs"/>
						<xsl:with-param name="count" select="$count+1"/>
						<xsl:with-param name="colnum">
							<xsl:choose>
								<xsl:when test="$colspec/@colnum">
									<xsl:value-of select="$colspec/@colnum + 1"/>
								</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="$colnum + 1"/>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:with-param>
					</xsl:call-template>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:otherwise>
	</xsl:choose>
</xsl:template>

<xsl:template name="generate.col">
	<!-- generate the table-column for column countcol -->
	<xsl:param name="countcol">1</xsl:param>
	<xsl:param name="colspecs" select="./colspec"/>
	<xsl:param name="count">1</xsl:param>
	<xsl:param name="colnum">1</xsl:param>

	<xsl:choose>
		<xsl:when test="$count>count($colspecs)">
			<fo:table-column column-number="{$countcol}">
				<xsl:variable name="colwidth">
					<xsl:call-template name="calc.column.width"/>
				</xsl:variable>
				<xsl:if test="$colwidth != 'proportional-column-width(1)' or 	$inhibit-default-colwidth-emission='0'">
					<xsl:attribute name="column-width">
						<xsl:value-of select="$colwidth"/>
					</xsl:attribute>
				</xsl:if>
			</fo:table-column>
		</xsl:when>
		<xsl:otherwise>
			<xsl:variable name="colspec" select="$colspecs[$count=position()]"/>

			<xsl:variable name="colspec.colnum">
				<xsl:choose>
					<xsl:when test="$colspec/@colnum">
						<xsl:value-of select="$colspec/@colnum"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="$colnum"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:variable>

			<xsl:variable name="colspec.colwidth">
				<xsl:choose>
					<xsl:when test="$colspec/@colwidth='*'">1*</xsl:when>
					<xsl:when test="$colspec/@colwidth">
						<xsl:value-of select="$colspec/@colwidth"/>
					</xsl:when>
					<xsl:otherwise>1*</xsl:otherwise>
				</xsl:choose>
			</xsl:variable>

			<xsl:choose>
				<xsl:when test="$colspec.colnum=$countcol">
					<fo:table-column column-number="{$countcol}">
						<xsl:variable name="colwidth">
							<xsl:call-template name="calc.column.width">
								<xsl:with-param name="colwidth">
									<xsl:value-of select="$colspec.colwidth"/>
								</xsl:with-param>
							</xsl:call-template>
						</xsl:variable>
						<xsl:if test="$colwidth != 'proportional-column-width(1)' or 			$inhibit-default-colwidth-emission='0'">
							<xsl:attribute name="column-width">
								<xsl:value-of select="$colwidth"/>
							</xsl:attribute>
						</xsl:if>
					</fo:table-column>
				</xsl:when>
				<xsl:otherwise>
					<xsl:call-template name="generate.col">
						<xsl:with-param name="countcol" select="$countcol"/>
						<xsl:with-param name="colspecs" select="$colspecs"/>
						<xsl:with-param name="count" select="$count+1"/>
						<xsl:with-param name="colnum">
							<xsl:choose>
								<xsl:when test="$colspec/@colnum">
									<xsl:value-of select="$colspec/@colnum + 1"/>
								</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="$colnum + 1"/>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:with-param>
					</xsl:call-template>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:otherwise>
	</xsl:choose>
</xsl:template>

<xsl:template name="calc.column.width">
	<xsl:param name="colwidth">1*</xsl:param>

	<!-- Ok, the colwidth could have any one of the following forms: -->
	<!--        1*       = proportional width -->
	<!--     1unit       = 1.0 units wide -->
	<!--         1       = 1pt wide -->
	<!--  1*+1unit       = proportional width + some fixed width -->
	<!--      1*+1       = proportional width + some fixed width -->

	<!-- If it has a proportional width, translate it to XSL -->
	<xsl:if test="contains($colwidth, '*')">
		<xsl:text>proportional-column-width(</xsl:text>
		<xsl:value-of select="substring-before($colwidth, '*')"/>
		<xsl:text>)</xsl:text>
	</xsl:if>

	<!-- Now grab the non-proportional part of the specification -->
	<xsl:variable name="width-units">
		<xsl:choose>
			<xsl:when test="contains($colwidth, '*')">
				<xsl:value-of select="normalize-space(substring-after($colwidth, '*'))"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="normalize-space($colwidth)"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>

	<!-- Ok, now the width-units could have any one of the following forms: -->
	<!--                 = <empty string> -->
	<!--     1unit       = 1.0 units wide -->
	<!--         1       = 1pt wide -->
	<!-- with an optional leading sign -->

	<!-- Grab the width part by blanking out the units part and discarding -->
	<!-- whitespace. It's not pretty, but it works. -->
	<xsl:variable name="width"
		select="normalize-space(translate($width-units,
		'+-0123456789.abcdefghijklmnopqrstuvwxyz',
	'+-0123456789.'))"/>

	<!-- Grab the units part by blanking out the width part and discarding -->
	<!-- whitespace. It's not pretty, but it works. -->
	<xsl:variable name="units"
		select="normalize-space(translate($width-units,
		'abcdefghijklmnopqrstuvwxyz+-0123456789.',
	'abcdefghijklmnopqrstuvwxyz'))"/>

	<!-- Output the width -->
	<xsl:value-of select="$width"/>

	<!-- Output the units, translated appropriately -->
	<xsl:choose>
		<xsl:when test="$units = 'pi'">pc</xsl:when>
		<xsl:when test="$units = '' and $width != ''">pt</xsl:when>
		<xsl:otherwise>
			<xsl:value-of select="$units"/>
		</xsl:otherwise>
	</xsl:choose>
</xsl:template>

<!-- The templates above call various named templates to set style
properties on various table related FOs.  When this file is
used via TurboStyler, appropriately defined named templates
are emitted by TurboStyler.

If this file is used in another context, the following (commented out)
empty named template definitions can be used:
-->
<xsl:template name="tgroup.first"/>

<xsl:template name="tgroup.notfirst"/>

<xsl:template name="thead"/>

<xsl:template name="tfoot"/>

<xsl:template name="tbody"/>

<xsl:template name="row"/>

<!-- Template to emit _cdfod:*="1" att specs for all possible
structural table attributes so that they are seen as "direct"
and not deleted in the case that there is a style-reference -->
<xsl:template name="maybe-emit-rtf-direct-attrs">
	<xsl:if test="$doing-cdfo-processing='1'"
		xmlns:_cdfod="http://www.arbortext.com/namespace/Styler/RTFInternalUse">
		<xsl:attribute name="_cdfod:margin-left">1</xsl:attribute>
		<xsl:attribute name="_cdfod:margin-right">1</xsl:attribute>
		<xsl:attribute name="_cdfod:padding">1</xsl:attribute>
		<xsl:attribute name="_cdfod:text-align">1</xsl:attribute>
		<xsl:attribute name="_cdfod:display-align">1</xsl:attribute>
		<xsl:attribute name="_cdfod:padding-left">1</xsl:attribute>
		<xsl:attribute name="_cdfod:padding-right">1</xsl:attribute>
		<xsl:attribute name="_cdfod:padding-top">1</xsl:attribute>
		<xsl:attribute name="_cdfod:padding-bottom">1</xsl:attribute>
		<xsl:attribute name="_cdfod:border-left-style">1</xsl:attribute>
		<xsl:attribute name="_cdfod:border-left-width">1</xsl:attribute>
		<xsl:attribute name="_cdfod:border-left-color">1</xsl:attribute>
		<xsl:attribute name="_cdfod:border-right-style">1</xsl:attribute>
		<xsl:attribute name="_cdfod:border-right-width">1</xsl:attribute>
		<xsl:attribute name="_cdfod:border-right-color">1</xsl:attribute>
		<xsl:attribute name="_cdfod:border-top-style">1</xsl:attribute>
		<xsl:attribute name="_cdfod:border-top-width">1</xsl:attribute>
		<xsl:attribute name="_cdfod:border-top-color">1</xsl:attribute>
		<xsl:attribute name="_cdfod:border-bottom-style">1</xsl:attribute>
		<xsl:attribute name="_cdfod:border-bottom-width">1</xsl:attribute>
		<xsl:attribute name="_cdfod:border-bottom-color">1</xsl:attribute>
		<xsl:attribute name="_cdfod:number-rows-spanned">1</xsl:attribute>
		<xsl:attribute name="_cdfod:number-columns-spanned">1</xsl:attribute>
		<xsl:attribute name="_cdfod:background-color">1</xsl:attribute>
		<xsl:attribute name="_cdfod:height">1</xsl:attribute>
	</xsl:if>
</xsl:template>

<!-- **********************************************************
end tbl.xsl
********************************************************** -->

<!-- ==================================================================== -->
<!-- ==================================================================== -->
<xsl:template name="setCellColor">
	<!-- context is entry-->
	<!--e.g. <?Pub _cellfont FontColor="white" Shading="#000000"?> -->
	<xsl:if test="contains(processing-instruction()[name()='Pub'], '_cellfont')">
		<xsl:variable name="tmp" select="processing-instruction()[contains(.,  '_cellfont')]"/>

		<xsl:analyze-string select="$tmp" regex="(FontColor=&quot;)([a-z]+[0-9]?)(&quot;)" flags="i">
			<xsl:matching-substring>
				<saxon:assign name="cellfontcolor">
					<xsl:call-template name="epic-color-to-hex">
						<xsl:with-param name="epic-color" select="lower-case(regex-group(2))"/>
					</xsl:call-template>
				</saxon:assign>
			</xsl:matching-substring>
			<xsl:non-matching-substring>
				<xsl:analyze-string select="." regex="(FontColor=&quot;)(&#35;([0-9]|[A-Z])+)(&quot;)" flags="i">
					<xsl:matching-substring>
						<saxon:assign name="cellfontcolor">
							<xsl:value-of select="regex-group(2)"/>
						</saxon:assign>
					</xsl:matching-substring>
				</xsl:analyze-string>
			</xsl:non-matching-substring>
		</xsl:analyze-string>

		<xsl:analyze-string select="$tmp" regex="(Shading=&quot;)([a-z]+[0-9]?)(&quot;)" flags="i">
			<xsl:matching-substring>
				<saxon:assign name="cellbgcolor">
					<xsl:call-template name="epic-color-to-hex">
						<xsl:with-param name="epic-color" select="lower-case(regex-group(2))"/>
					</xsl:call-template>
				</saxon:assign>
			</xsl:matching-substring>
			<xsl:non-matching-substring>
				<xsl:analyze-string select="$tmp" regex="(Shading=&quot;)(&#35;([0-9]|[a-z])+)(&quot;)" flags="i">
					<xsl:matching-substring>
						<saxon:assign name="cellbgcolor">
							<xsl:value-of select="regex-group(2)"/>
						</saxon:assign>
					</xsl:matching-substring>
					<!--xsl:non-matching-substring>
					<xsl:message>non-matching:<xsl:value-of select="$tmp"/>
					</xsl:message>
				</xsl:non-matching-substring-->
			</xsl:analyze-string>

		</xsl:non-matching-substring>
	</xsl:analyze-string>

	<xsl:analyze-string select="$tmp" regex="(Shading=&quot;)(&#35;[0-9]+)(&quot;)">
		<xsl:matching-substring>
			<saxon:assign name="cellbgcolor">
				<xsl:value-of select="regex-group(2)"/>
			</saxon:assign>
		</xsl:matching-substring>
		<xsl:non-matching-substring>
			<xsl:analyze-string select="$tmp" regex="(Shading=&quot;)([a-z]+[0-9]?)(&quot;)">
				<xsl:matching-substring>
					<saxon:assign name="cellbgcolor">
						<xsl:call-template name="epic-color-to-hex">
							<xsl:with-param name="epic-color" select="regex-group(2)"/>
						</xsl:call-template>
					</saxon:assign>
				</xsl:matching-substring>

			</xsl:analyze-string>
		</xsl:non-matching-substring>
	</xsl:analyze-string>

</xsl:if>
</xsl:template>


<!-- Template to convert epic color specification to FO color -->
<!-- Possible epic colors are: aqua, black, blue, brown, gray1, gray2, gray3, gray4, gray5,
green, lime, maroon, navy, olive, orange, red, teal, white, yellow, and violet -->
<xsl:template name="epic-color-to-hex">
	<xsl:param name="epic-color"/>
	<xsl:choose>
		<xsl:when test="$epic-color = 'black' ">
			<xsl:value-of select="'#000000'"/>
		</xsl:when>
		<xsl:when test="$epic-color = 'maroon' ">
			<xsl:value-of select="'#800000'"/>
		</xsl:when>
		<xsl:when test="$epic-color = 'teal' ">
			<xsl:value-of select="'#008080'"/>
		</xsl:when>
		<xsl:when test="$epic-color = 'blue' ">
			<xsl:value-of select="'#0000FF'"/>
		</xsl:when>
		<xsl:when test="$epic-color = 'navy' ">
			<xsl:value-of select="'#000080'"/>
		</xsl:when>

		<xsl:when test="$epic-color = 'red' ">
			<xsl:value-of select="'#FF0000'"/>
		</xsl:when>
		<xsl:when test="$epic-color = 'brown' ">
			<xsl:value-of select="'#803F00'"/>
		</xsl:when>
		<xsl:when test="$epic-color = 'olive' ">
			<xsl:value-of select="'#808000'"/>
		</xsl:when>
		<xsl:when test="$epic-color = 'green' ">
			<xsl:value-of select="'#008000'"/>
		</xsl:when>
		<xsl:when test="$epic-color = 'aqua' ">
			<xsl:value-of select="'#00FFFF'"/>
		</xsl:when>

		<xsl:when test="$epic-color = 'violet' ">
			<xsl:value-of select="'#FF00FF'"/>
		</xsl:when>
		<xsl:when test="$epic-color = 'orange' ">
			<xsl:value-of select="'#FF7F00'"/>
		</xsl:when>
		<xsl:when test="$epic-color = 'yellow' ">
			<xsl:value-of select="'#FFFF00'"/>
		</xsl:when>
		<xsl:when test="$epic-color = 'lime' ">
			<xsl:value-of select="'#00FF00'"/>
		</xsl:when>
		<xsl:when test="$epic-color = 'white' ">
			<xsl:value-of select="'#FFFFFF'"/>
		</xsl:when>

		<xsl:when test="$epic-color = 'gray1' ">
			<xsl:value-of select="'#E0E0E0'"/>
		</xsl:when>
		<xsl:when test="$epic-color = 'gray2' ">
			<xsl:value-of select="'#D0D0D0'"/>
		</xsl:when>
		<xsl:when test="$epic-color = 'gray3' ">
			<xsl:value-of select="'#C0C0C0'"/>
		</xsl:when>
		<xsl:when test="$epic-color = 'gray4' ">
			<xsl:value-of select="'#B0B0B0'"/>
		</xsl:when>
		<xsl:when test="$epic-color = 'gray5' ">
			<xsl:value-of select="'#808080'"/>
		</xsl:when>
		<xsl:when test="$epic-color = 'default' ">
			<xsl:value-of select="''"/>
		</xsl:when>
		<xsl:otherwise>
			<!-- use the same name -->
			<xsl:value-of select="$epic-color"/>
		</xsl:otherwise>
	</xsl:choose>
</xsl:template>
</xsl:stylesheet>
