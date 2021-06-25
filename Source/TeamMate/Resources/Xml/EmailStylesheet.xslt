<?xml version="1.0" encoding="utf-8"?>

<!-- 
*******************************************************************************
* This stylesheet transforms a threaded discussion into a formatted HTML table
*******************************************************************************
-->

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns="http://www.w3.org/1999/xhtml">
  
  <xsl:output method="html" indent="yes"/>

  <!-- ******************************************************************** -->
  <!-- Parameters                                                           -->
  <!-- ******************************************************************** -->

  <!-- Values could be: Outlook, Plain -->
  <xsl:param name="Mode">Plain</xsl:param>
  <xsl:param name="NoThumbnail"/>

  <!-- ******************************************************************** -->
  <!-- Global Variables                                                     -->
  <!-- ******************************************************************** -->

  <xsl:variable name="IdFieldName">System.Id</xsl:variable>
  <xsl:variable name="TitleFieldName">System.Title</xsl:variable>

  <!-- ******************************************************************** -->
  <!-- Discussion Root Template                                             -->
  <!-- ******************************************************************** -->
  
  <!-- NOTE: <br/>s at the beginning and end of <body> are by design, they play better
       with email editing once you are inside the Outlook editor window .-->

  <xsl:template match="/Discussion">
    <html>
      <head>
        <style type="text/css">
          <xsl:call-template name="Stylesheet"/>
        </style>
      </head>
      <body>
        <br/>
        <br/>
        <hr/>
        <xsl:apply-templates select="WorkItemCollection" mode="Body"/>
        <br/>

        <xsl:apply-templates select="Fields/Field"/>

        <xsl:if test="Revision">
          <div class="section">Discussion</div>
          <table cellpadding="0" class="discussion">
            <xsl:apply-templates select="Revision"/>
          </table>
        </xsl:if>
        <br/>
      </body>
    </html>
  </xsl:template>

  <xsl:template match="Field">
    <div class="section">
      <xsl:value-of select="@Name"/>
    </div>
    <div style="margin-bottom: 10px">
      <xsl:value-of select="." disable-output-escaping="yes"/>
    </div>
  </xsl:template>

  <xsl:template match="Revision">
    <xsl:variable name="thumbnailPath">
      <xsl:apply-templates select="." mode="ThumbnailPath"/>
    </xsl:variable>

    <tr>
      <td class="thumbnailcontainer">
        <xsl:if test="$thumbnailPath">
          <img class="thumbnail" src="{$thumbnailPath}" height="32"/>
        </xsl:if>
      </td>
      <td>
        <span class="tagline">
          <span class="name">
            <xsl:value-of select="ChangedBy"/>
          </span>
          <span class="date">
            <xsl:value-of select="ChangedDate"/>
          </span>
        </span>
        <br/>
        <div class="history">
          <xsl:value-of select="History" disable-output-escaping="yes"/>
        </div>
      </td>
    </tr>
  </xsl:template>

  <xsl:template match="Revision" mode="ThumbnailPath">
    <xsl:variable name="path">
      <xsl:choose>
        <xsl:when test="Thumbnail">
          <xsl:value-of select="Thumbnail"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$NoThumbnail"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:choose>
      <xsl:when test="$Mode = 'Outlook' and $path">
        <xsl:value-of select="concat('cid:', $path)"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$path"/>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <!-- ******************************************************************** -->
  <!-- WorkItemCollection Root Template -->
  <!-- ******************************************************************** -->

  <xsl:template match="/WorkItemCollection">
    <html>
      <head>
        <style type="text/css">
          <xsl:call-template name="Stylesheet"/>
        </style>
      </head>
      <body>
        <br />
        <br />
        <xsl:apply-templates select="." mode="Body"/>
        <br />
      </body>
    </html>
  </xsl:template>

  <xsl:template match="WorkItemCollection" mode="Body">
    <xsl:apply-templates select="Info"/>
    <xsl:apply-templates select="." mode="Table"/>
  </xsl:template>

  <xsl:template match="Info">
    <xsl:if test="Query">
      <table class="headertable">
        <xsl:if test="Query">
          <tr>
            <td class="header">Query:</td>
            <td>
              <xsl:choose>
                <xsl:when test="Query/@URL">
                  <a href="{Query/@URL}">
                    <xsl:value-of select="Query"/>
                  </a>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="Query"/>
                </xsl:otherwise>
              </xsl:choose>
            </td>
          </tr>
        </xsl:if>
      </table>
      <br/>
    </xsl:if>
  </xsl:template>

  <xsl:template match="WorkItemCollection" mode="Table">
    <xsl:variable name="headers" select="Fields/Field"/>
    
    <table class="workitemtable" cellpadding="3">
      <tr>
        <xsl:for-each select="Fields/Field">
          <th>
            <xsl:value-of select="."/>
          </th>
        </xsl:for-each>
      </tr>

      <xsl:for-each select="WorkItems/WorkItem">
        <xsl:variable name="url" select="@URL"/>
        <xsl:variable name="indentation" select="@Indentation"/>
        <tr>
          <xsl:choose>
            <xsl:when test="position() mod 2 != 1">
              <xsl:attribute name="class">row1</xsl:attribute>
            </xsl:when>
            <xsl:otherwise>
              <xsl:attribute name="class">row2</xsl:attribute>
            </xsl:otherwise>
          </xsl:choose>

          <xsl:for-each select="Value">
            <xsl:variable name="position" select="position()"/>
            <xsl:variable name="header" select="$headers[$position]"/>
            <xsl:variable name="isId" select="$header[@ReferenceName=$IdFieldName]"/>
            <xsl:variable name="isTitle" select="$header[@ReferenceName=$TitleFieldName]"/>
            <td>
              <xsl:if test="$isTitle">
                <xsl:attribute name="class">Title</xsl:attribute>
              </xsl:if>
              <xsl:if test="$indentation and $isTitle">
                <xsl:variable name="padding" select="concat($indentation * 20 + 6, 'px')"/>
                <xsl:attribute name="style">padding-left: <xsl:value-of select="$padding"/>;</xsl:attribute>
              </xsl:if>
              <xsl:choose>
                <xsl:when test="$url and $isId">
                  <a href="{$url}">
                    <xsl:value-of select="."/>
                  </a>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="."/>
                </xsl:otherwise>
              </xsl:choose>
            </td>
            
          </xsl:for-each>
        </tr>
      </xsl:for-each>
    </table>
  </xsl:template>

  <!-- ******************************************************************** -->
  <!-- Stylesheet                                                           -->
  <!-- ******************************************************************** -->

  <xsl:template name="Stylesheet" xml:space="preserve">
    body, table
    {
      font-family: Calibri, Sans-Serif; 
      font-size: 11pt;
    }
    
    table
    {
      border-collapse: collapse;
      border-spacing: 0;
    }

    /* Used for the work item table */
    
    .row1
    {
      background: #F0F4FA;
    }
    
    .row2
    {
      background: #DEE8F2;
    }
    
    .headertable
    {
      color: #3d5277;
    }
    
    .header
    {
      font-weight: bold;
      padding-right: 20px;
    }
    
    .workitemtable tr th
    {
      background: #3D5277;
      font-weight: bold;
      color: white;
    }
    
    .workitemtable tr td, .workitemtable tr th
    {
      border-right: 1px solid white;
      white-space: nowrap;
      text-align: left;
      vertical-align: text-top;
      padding-left: 6px;
      padding-right: 6px;
    }
    
    .workitemtable tr td.Title
    {
      white-space: normal;
    }
    
    /* Used for the discussion */

    table.discussion tr
    {
      vertical-align: text-top;
      padding-bottom: 20px;
    }
    
    .thumbnail
    {
      /* Thumbnail height optimized to fit tag line + 1 line comment */
      cursor: pointer;
      border: 1px solid #7D7D7D;
    }
    
    .tagline
    {
      font-size: 90%;
    }
    
    .history
    {
      padding-top: 6;
      padding-left: 10px;
    }
    
    .name
    {
      font-weight: bold;
    }
    
    .date
    {
      color: gray;
    }
    
    .thumbnailcontainer
    {
      text-align: center;
      padding-right: 10px;
    }
    
    .section
    {
      font-weight: bold;
      font-size: 120%;
      text-transform: uppercase;
      margin-bottom: 3px;
    }
    
    p
    {
      margin-bottom: 0;
      margin-top: 0;
    }
    
  </xsl:template>

</xsl:stylesheet>
