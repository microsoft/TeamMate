<?xml version="1.0" encoding="utf-8"?>

<!-- 
*******************************************************************************
* This stylesheet transforms CodeFlow reviews that need to be completed into
* an Outlook reminder email.
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
  <xsl:param name="Name"></xsl:param>

  <!-- ******************************************************************** -->
  <!-- Global Variables                                                     -->
  <!-- ******************************************************************** -->

  <!-- ******************************************************************** -->
  <!-- Discussion Root Template                                             -->
  <!-- ******************************************************************** -->

  <xsl:template match="/*">
    <html>
      <head>
        <style type="text/css">
          <xsl:call-template name="Stylesheet"/>
        </style>
      </head>
      <body>

        <div>
          <p>Please look at the code reviews below and take time to mark the appropriate ones as completed.</p>
          <p>When completing reviews, please follow a good code review etiquette:</p>
          <ul>
            <li>
              <b>Code reviews SHOULD NOT be completed with ANY comments in Active state.</b>
              <ul>
                <li>
                  If you do, reviewers have no clue about what decisions you've made based on their feedback. Please don't do this.
                </li>
              </ul>
              </li>
            <li class="spaced">
              <b>
                Please make sure to walk through active comments, and mark them as either Resolved, Pending or Won't Fix.
              </b>
              <ul>
                <li>
                  <b>Resolved</b> means that you have made a code change based on the suggestion.
                </li>
                <li>
                  <b>Pending</b> means that further thinking or discussion is needed. This item might not be addressed as part of your immediate check in.
                </li>
                <li>
                  <b>Won't Fix</b> means that you will not be making a change to the code base on the particular suggestion or question.
                </li>
              </ul>
            </li>
            <li class="spaced">
              <b>Pending and Won't Fix resolutions MUST HAVE comments attached to the resolution.</b>
              <ul>
                <li>
                  It is not acceptable to simply mark something as Pending or Won't Fix without explaining the rationale behind it.
                </li>
                <li>
                  Somebody spent their time giving you the feedback that you requested, please spend your time with clear replies.
                </li>
              </ul>
            </li>
          </ul>

          <p>
            Thanks in advance!
            <xsl:if test="$Name">
              <br/>
              <xsl:value-of select="$Name"/>
            </xsl:if>
          </p>
        </div>

        <hr style="height: 1pt"/>
        
        <xsl:for-each select="ReviewGroup">
          <div>
            <div class="section">
              <xsl:value-of select="Author/@DisplayName"/> (<xsl:value-of select="count(Reviews/Review)"/>)
            </div>
            <table class="reviews">
              <tr>
                <th style="width: 65%; text-align: left">Name</th>
                <th style="width: 15%; text-align: left">Updated</th>
                <th style="width: 20%; text-align: left">Participants</th>
              </tr>
              <xsl:for-each select="Reviews/Review">
                <xsl:variable name="rowClass">
                  <xsl:choose>
                    <xsl:when test="position() mod 2 != 0">row1</xsl:when>
                    <xsl:otherwise>row2</xsl:otherwise>
                  </xsl:choose>
                </xsl:variable>
                
                <tr class="{$rowClass}">
                  <td class="nameColumn">
                    <a href="{CodeFlowUrl}">
                      <xsl:value-of select="Name"/>
                    </a>
                  </td>
                  <td class="updatedColumn">
                    <xsl:value-of select="FriendlyLastUpdatedOn"/>
                  </td>
                  <td class="participantsColumn">
                    <xsl:apply-templates select="." mode="Participants"/>
                  </td>
                </tr>
              </xsl:for-each>
            </table>
            <div style="margin-bottom: 15px">&#160;</div>
          </div>
        </xsl:for-each>
      </body>
    </html>
  </xsl:template>

  <xsl:template match="Review" mode="Participants">
    <xsl:variable name="all2">
      <xsl:copy-of select="Reviewers/Reviewer[@Status='Waiting']"/>
      <xsl:copy-of select="Reviewers/Reviewer[@Status='SignedOff']"/>
      <xsl:copy-of select="Reviewers/Reviewer[@Status='Reviewing']"/>
      <xsl:copy-of select="Reviewers/Reviewer[@Status='Started']"/>
    </xsl:variable>

    <xsl:for-each select="msxsl:node-set($all2)/*">
      <xsl:if test="position() &gt; 1">, </xsl:if>
      <span class="{@Status}">
        <xsl:value-of select="@Name"/>
      </span>
    </xsl:for-each>
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

    a
    {
      text-decoration: none;
      color: #4183c4;
    }

    a:hover
    {
      text-decoration: underline;
    }
    
    .section
    {
      font-weight: bold;
      font-size: 120%;
      text-transform: uppercase;
      margin-top: 10px;
      margin-bottom: 10px;
    }

    li.spaced
    {
      margin-top: 12pt;
    }
    
    /* Reviews Table */

    table.reviews
    {
      margin: 0;
      width: 100%;
      border: 1px solid #EAEAEA;
    }
    
    table.reviews tr th
    {
      text-align: left;
      background: #E2E2E2;
    }

    table.reviews tr td
    {
      vertical-align: top;
    }

    table.reviews tr th, table.reviews tr td
    {
      padding: 5px 5px 5px 5px;
      border-bottom: 1px solid #EAEAEA;
    }

    .row1
    {
      background: white;
    }
    
    .row2
    {
      background: #F9F9F9;
    }
    
    .nameColumn a
    {
    /*
      color: black;
      font-weight: bold;
      */
    }
    
    .updatedColumn
    {
      color: #888;
    }

    /* ReviewerStatus */

    .SignedOff
    {
      font-weight: bold;
      color: green;
    }
    
    .Waiting
    {
      font-weight: bold;
      color: red;
    }
    
    .reviewsDiv
    {
      margin-bottom: 30px;
    }

  </xsl:template>

</xsl:stylesheet>
