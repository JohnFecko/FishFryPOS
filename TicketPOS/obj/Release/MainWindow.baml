   M S B A M L   `   `   `  ���� D  @TicketPOS, Version=2.0.0.0, Culture=neutral, PublicKeyToken=null%clr-namespace:TicketPOS	TicketPOS      TicketPOS.MainWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     )localclr-namespace:TicketPOS     -     � Title$  
Ticket POS��$	��750��$	��900��.��� ���	#FFE8E0E0.+��� $��	50,0,50,0q�.+��� -    ��Name$ groupTicket��$��Table Number��$��75��$	��750��$��Center=�$��18(�$
��21,0q�.���� -    ��Name$ 	txtTicket�� �� TextWrapping$
 Wrap=�$��18(�$��	10,8,10,4q��� -   $ 	groupName��$��	Last Name��$��85��$	��750��$��Center=�$��18(�$
��21,0q�.���� -    ��Name$ 	panelName��$��60��$��0,0,0,0q�$	��Top=�.+�   TicketPOS.CustomComboBox  -     Name$ 	comboName��   TicketPOS.CustomComboBox   SelectionChanged$  comboName_SelectionChanged��   KeyUp$ comboName_KeyUp�� �� 
IsEditable . $��18(�$
��10,8q��� -   $ txtName��$��34��$
 Wrap=�$��18(�$
��10,8q��� -   $ 
groupTimes��$��Times��$	��116��$	��750��$��Center=�$��18(�$
��21,0q�.���� -	   $ 
panelTimes��$��91��$��0,0,0,0q�$	��Top=�.+��� -
   	 ��Name$	 
comboDates��$��18(�$
��10,8q�
 �� ItemStringFormat$
 d���� -   $	 
comboTimes��$��18(�$
��10,8q�$
 t���� -   $ 
groupItems��$��Items��$��18(�$��Center=�$	��750��.���� -   $ 
panelItems��$��	10,0,10,0q� �� Orientation$ Vertical=��� $��18(�$��Center=�$	��750��$��53��$
��21,0q�.���� $ 
Horizontal=�$��Right=�.+�� $��37��$	��Top=�$��Right=�.+��� -    ��Name$ label��$��Total Price��$��18(�$��Right=�$��0,0,20,0q��� -   $ txtTotal��$
 Wrap=�$�� ��$	��120�� �� 
IsReadOnly . $��18(�$��Right=�$��30��$��0,4,0,0q��� -   $ 
groupNotes��$��Notes��$��75��$	��750��$��Center=�$��18(�$
��21,0q�.���� -   $ txtNotes��$
 Wrap=�$��14(�$��	10,0,10,0q� �� AcceptsReturn . " �� VerticalScrollBarVisibility$
 Auto=��� -   $ 	groupToGo��$��18(�$��Center=�$	��750��$
��21,0q�.���� $ Vertical=�$��Center=�.+��� $ 
Horizontal=�$��Center=�.+��� -    ��Name$ chkToGo��$��Take Out Order��$��0,15,0,0q�$��12(� � LayoutTransform ��  �� ScaleX$	 1.5Q� �� ScaleY$	 1.5Q��� $ 
Horizontal=�$��Center=�$��0,15,0,0q�.+��� -   $��btnScan��$��Scan Ticket��$��	20,5,20,5q��� -    ��Name$ button��$��Complete Order��$��	20,5,20,5q�$��Bottom=�$��18(�$��Center=��� $ 
Horizontal=�$��Right=�$��0,10,0,0q�.+��� -   $ 	btnCancel��$��Cancel Order��$��	20,5,20,5q�$��Bottom=�$��18(�$��0,0,10,0q��� $ 
Horizontal=�$��Right=�.+��� -   $ btnPrintReport��$��Reports��$��	20,5,20,5q�$��Bottom=�$��18(�$��0,0,10,0q��� -   $ btnQuickSale��$��
Quick Sale��$��	20,5,20,5q�$��Bottom=�$��18(�