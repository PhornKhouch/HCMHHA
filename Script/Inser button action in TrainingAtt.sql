----insert btn------
insert SYScreenActionRow(TemplateID,IsAction,ActionInCode,ActionId,ActionName,Text,LinkMode,ToolTip,InOrder,GroupAction,GroupActionName,DefaultJSScript)
          values('LIST_ACTION_NORMAL','IsAtt','Attendence','_btnAtt','Index','Attendence','L','Attendence','1','ACTIONS','ACTIONS','ActionItemClick')
insert SYScreenActionRow(TemplateID,IsAction,ActionInCode,ActionId,ActionName,Text,LinkMode,ToolTip,InOrder,GroupAction,GroupActionName,DefaultJSScript)
          values('LIST_ACTION_NORMAL','IsABS','Absent','_btnABS','Index','Absent','L','Absent','2','ACTIONS','ACTIONS','ActionItemClick')
---------------

INSERT INTO SYActionTemplate values('TR00000003','Attendence','EDIT_ACTION_NORMAL','Attendence')
INSERT INTO SYRoleItems values('5','TR00000003','Attendence','EDIT_ACTION_NORMAL','')
INSERT INTO SYActionTemplate values('TR00000003','Absent','EDIT_ACTION_NORMAL','Attendence')
INSERT INTO SYRoleItems values('5','TR00000003','Absent','EDIT_ACTION_NORMAL','')
-------end--------


----InsertMessage----

INSERT SYMessage VALUES('REL_ATT','EN','SY','Q','Are you sure to Attandance this document?',null)
-------end---------