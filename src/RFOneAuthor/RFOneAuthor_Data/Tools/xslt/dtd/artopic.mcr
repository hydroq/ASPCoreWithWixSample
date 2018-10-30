<?xml version="1.0"?>
<!DOCTYPE MACROS SYSTEM "macros.dtd">
<MACROS>
	<MACRO name="On_Macro_File_Load" lang="JScript" desc="Macro executed when the macro file is loaded" tooltip="" hide="false" id="">
	<![CDATA[
	  //Application.Alert("On_Macro_File_Load");
	]]>
	</MACRO>
	<MACRO name="ImageAsSVG_OnShouldCreate" lang="JScript" desc="" tooltip="" hide="false" id="">
	<![CDATA[
	  var aipc = ActiveDocument.ActiveInPlaceControl;
    if (aipc != null) 
    {
      aipc.ShouldCreate = false;
      var domnode = aipc.Node;
      var href = domnode.attributes.getNamedItem("href");
      if (href != null && href.value != null) 
      {
        aipc.ShouldCreate = /\.svg$/i.test(href.value);
      }
    }
	  //Application.Alert("ImageAsCGM_OnShouldCreate");
	]]>
	</MACRO>
	<MACRO name="ImageAsSVG_OnInitialize" lang="JScript" desc="" tooltip="" hide="false" id="">
	<![CDATA[
	  //Application.Alert("ImageAsSVG_OnInitialize");
	  var aipc = ActiveDocument.ActiveInPlaceControl;

    aipc.Width = 400;
	  aipc.Height = 400;

    var domnode = aipc.Node;
    var height = domnode.attributes.getNamedItem("height");
    if (height != null && height.value != null) 
      aipc.Height = height.value;
    var width = domnode.attributes.getNamedItem("width");
    if (width != null && width.value != null) 
      aipc.Width = width.value;
	  var path = ActiveDocument.FullName.split('\\').slice(0,-1).join('\\');
    var src = domnode.attributes.getNamedItem("href");
    if (src != null && src.value != null) 
      aipc.Control.Navigate(path + "\\" + src.value, 2);
	  //Application.Alert(src);
	]]>
	</MACRO>
	<MACRO name="ImageAsCGM_OnShouldCreate" lang="JScript" desc="" tooltip="" hide="false" id="">
	<![CDATA[
	  var aipc = ActiveDocument.ActiveInPlaceControl;
    if (aipc != null) 
    {
      aipc.ShouldCreate = false;
      var domnode = aipc.Node;
      var href = domnode.attributes.getNamedItem("href");
      if (href != null && href.value != null) 
      {
        aipc.ShouldCreate = /\.cgm$/i.test(href.value);
      }
    }
	  //Application.Alert("ImageAsCGM_OnShouldCreate");
	]]>
	</MACRO>
	<MACRO name="ImageAsCGM_OnInitialize" lang="JScript" desc="" tooltip="" hide="false" id="">
	<![CDATA[
	  //Application.Alert("ImageAsCGM_OnInitialize");
	  var aipc = ActiveDocument.ActiveInPlaceControl;

    aipc.Width = 400;
	  aipc.Height = 400;

    var domnode = aipc.Node;
    var height = domnode.attributes.getNamedItem("height");
    if (height != null && height.value != null) 
      aipc.Height = height.value;
    var width = domnode.attributes.getNamedItem("width");
    if (width != null && width.value != null) 
      aipc.Width = width.value;
	  var path = ActiveDocument.FullName.split('\\').slice(0,-1).join('\\');
    var src = domnode.attributes.getNamedItem("href");
    if (src != null && src.value != null) 
      aipc.Control.src = path + "\\" + src.value;
    try {
	  aipc.Control.inactive = true;
    } catch(e) {}
	  //Application.Alert(src);
	]]>
	</MACRO>
	<MACRO name="ImageAsCGM_OnFocus" lang="JScript" desc="" tooltip="" hide="false" id="">
	<![CDATA[
	  //Application.Alert("ImageAsCGM_OnFocus");
	]]>
	</MACRO>
	<MACRO name="ImageAsCGM_OnSynchronize" lang="JScript" desc="" tooltip="" hide="false" id="">
	<![CDATA[
    var aipc = Application.ActiveInPlaceControl;
    if (aipc.UpdateFromDocument) 
    {
      aipc.Width = 400;
  	  aipc.Height = 400;
  
      var domnode = aipc.Node;
      var height = domnode.attributes.getNamedItem("height");
      if (height != null && height.value != null) 
        aipc.Height = height.value;
      var width = domnode.attributes.getNamedItem("width");
      if (width != null && width.value != null) 
        aipc.Width = width.value;
  	  var path = ActiveDocument.FullName.split('\\').slice(0,-1).join('\\');
      var src = domnode.attributes.getNamedItem("href");
      if (src != null && src.value != null) 
        aipc.Control.src = path + "\\" + src.value;
    }
	]]>
	</MACRO>
	<MACRO name="tableAdd" lang="JScript" desc="" key="Ctrl+Shift+T" tooltip="" hide="false" id="">
	<![CDATA[
		var tableString = '<table><tgroup cols="2"><colspec colnum="1" colname="col1" colwidth="*"/><colspec colnum="2" colname="col2" colwidth="*"/><thead><row><entry colname="col1"></entry><entry colname="col2"></entry></row></thead><tbody><row><entry colname="col1"></entry><entry colname="col2"></entry></row></tbody></tgroup></table>';
		if (Selection.CanPaste(tableString,true)) {
			Selection.PasteStringWithInterpret(tableString);
		} else {
			ActiveDocument.Host.Alert("Cannot insert a TABLE element here.");
		}
//		Selection.InsertCALSTable(1,2,"TABLE",true,false);
	]]>
	</MACRO>
	<MACRO name="tableInsertColumnsRight" lang="JScript" desc="Inserts a table column to the right of the column containing the selection." key="Ctrl+Shift+C" tooltip="" hide="false" id="">
	<![CDATA[
		if (Selection.InContextOfType("Table")){
			Selection.InsertColumnsRight();
		}
	]]>
	</MACRO>
	<MACRO name="tableInsertColumnsLeft" lang="JScript" desc="Inserts a table column to the left of the column containing the selection" key="Ctrl+Shift+X" tooltip="" hide="false" id="">
	<![CDATA[
		if (Selection.InContextOfType("Table")){
			Selection.InsertColumnsLeft();
		}
	]]>
	</MACRO>
	<MACRO name="tableInsertRowsBelow" lang="JScript" desc="Inserts a table row below the row containing the selection." key="Ctrl+Shift+R" tooltip="" hide="false" id="">
	<![CDATA[
		if (Selection.InContextOfType("Table")){
			Selection.InsertRowsBelow();
		}
	]]>
	</MACRO>
	<MACRO name="tableInsertRowsAbove" lang="JScript" desc="Inserts a table row above the row containing the selection." key="Ctrl+Shift+E" tooltip="" hide="false" id="">
	<![CDATA[
		if (Selection.InContextOfType("Table")){
			Selection.InsertRowsAbove();
		}
	]]>
	</MACRO>
	<MACRO name="tableDeleteRow" lang="JScript" desc="Deletes the table row containing the selection." key="Ctrl+Shift+D" tooltip="" hide="false" id="">
	<![CDATA[
		if (Selection.InContextOfType("TableCell") && Selection.CanDelete){
			Selection.DeleteRow();
		}
	]]>
	</MACRO>
	<MACRO name="tableDeleteColumn" lang="JScript" desc="Deletes the table column containing the selection." key="Ctrl+Shift+F" tooltip="" hide="false" id="">
	<![CDATA[
		if (Selection.InContextOfType("TableCell") && Selection.CanDelete){
			Selection.DeleteColumn();
		}
	]]>
	</MACRO>
	
	<MACRO name="tableMergeCellDown" lang="JScript" desc="Custom macro." tooltip="" hide="false" id="">
	<![CDATA[
		if (Selection.InContextOfType("TableCell")) {
			Selection.MergeCellDown();
		}
	]]></MACRO>
	<MACRO name="tableMergeCellLeft" lang="JScript" desc="Custom macro." tooltip="" hide="false" id="">
	<![CDATA[
		if (Selection.InContextOfType("TableCell")) {
			Selection.MergeCellLeft();
		}
	]]></MACRO>
	
	<MACRO name="tableMergeCellRight" lang="JScript" desc="Custom macro." tooltip="" hide="false" id="">
	<![CDATA[
		if (Selection.InContextOfType("TableCell")) {
			Selection.MergeCellRight();
		}
	]]></MACRO>
	<MACRO name="tableMergeCellUp" lang="JScript" desc="Custom macro." tooltip="" hide="false" id="">
	<![CDATA[
		if (Selection.InContextOfType("TableCell")) {
			Selection.MergeCellUp();
		}
	]]></MACRO>
	<MACRO name="tableMoveColumnLeft" lang="JScript" desc="Custom macro." tooltip="" hide="false" id="">
	<![CDATA[
      if (Selection.InContextOfType("TableCell")) {
        Selection.MoveColumnLeft();
      }
	]]></MACRO>
	<MACRO name="tableMoveColumnRight" lang="JScript" desc="Custom macro." tooltip="" hide="false" id="">
	<![CDATA[
      if (Selection.InContextOfType("TableCell")) {
        Selection.MoveColumnRight();
      }
	]]></MACRO>
	<MACRO name="tableMoveRowDown" lang="JScript" desc="Custom macro." tooltip="" hide="false" id="">
	<![CDATA[
      if (Selection.InContextOfType("TableCell")) {
        Selection.MoveRowDown();
      }
	]]></MACRO>
	
	<MACRO name="tableMoveRowUp" lang="JScript" desc="Custom macro." tooltip="" hide="false" id="">
	<![CDATA[
      if (Selection.InContextOfType("TableCell")) {
        Selection.MoveRowUp();
      }
	]]></MACRO>
	
	<MACRO name="tableSplitCellIntoColumns" lang="JScript" key="Ctrl+Shift+S" tooltip="" hide="false" id="">
	<![CDATA[
      if (Selection.InContextOfType("TableCell")) {
        Selection.SplitCellColumn();
      }
	]]></MACRO>
	<MACRO name="tableSplitCellIntoRows" lang="JScript" key="Ctrl+Shift+A" tooltip="" hide="false" id="">
	<![CDATA[
      if (Selection.InContextOfType("TableCell")) {
        Selection.SplitCellRow();
      }
	]]></MACRO>
	
	<MACRO name="On_Update_UI" lang="JScript" hide="true" desc="UI status change"><![CDATA[
      function changeMacroState(eName, bFlag) {
        ActiveDocument.CustomProperties.add("cmdui_e_" + eName, bFlag);
      }
      changeMacroState("tableAdd", (Selection.CanInsert("table") || Selection.CanInsert("tgroup")));
      changeMacroState("tableInsertRowsAbove", Selection.InContextOfType("Table"));
      changeMacroState("tableInsertRowsBelow", Selection.InContextOfType("Table"));
      changeMacroState("tableInsertColumnsLeft", Selection.InContextOfType("Table"));
      changeMacroState("tableInsertColumnsRight", Selection.InContextOfType("Table"));
      changeMacroState("tableDeleteRow", (Selection.InContextOfType("TableCell") && Selection.CanDelete));
      changeMacroState("tableDeleteColumn", (Selection.InContextOfType("TableCell") && Selection.CanDelete));
      changeMacroState("tableMoveRowUp", Selection.InContextOfType("TableCell"));
      changeMacroState("tableMoveRowDown", Selection.InContextOfType("TableCell"));
      changeMacroState("tableMoveColumnLeft", Selection.InContextOfType("TableCell"));
      changeMacroState("tableMoveColumnRight", Selection.InContextOfType("TableCell"));
      changeMacroState("tableMergeCellUp", Selection.InContextOfType("TableCell"));
      changeMacroState("tableMergeCellDown", Selection.InContextOfType("TableCell"));
      changeMacroState("tableMergeCellLeft", Selection.InContextOfType("TableCell"));
      changeMacroState("tableMergeCellRight", Selection.InContextOfType("TableCell"));
      changeMacroState("tableSplitCellIntoColumns", Selection.InContextOfType("TableCell"));
      changeMacroState("tableSplitCellIntoRows", Selection.InContextOfType("TableCell"));
	]]>
	</MACRO>
</MACROS>
