   M S B A M L   `   `   `  ����D  @TicketPOS, Version=2.0.0.0, Culture=neutral, PublicKeyToken=null%clr-namespace:TicketPOS	TicketPOS  5   	       TicketPOS.ReportsWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   )localclr-namespace:TicketPOS     5   	   $	��750��5      $	��900��6'     � ForceCursor  .  � WindowStyle$ 
ToolWindow=�6F    � WindowState$ 	Maximized=�6_   .��5      � -   5        ���	#FFE8E0E0.+�5	   
   (  "System.Windows.Controls.DatePicker  -   5        $��	dateStart��5	      $
��Left=�6&   $��	10,44,0,0q�6A   $	��Top=�6T   $	��150��6l   $��18(�6x     5
   
   -   5        $��dateEnd��5
      $
��Left=�6%   $��
205,44,0,0q�6@   $	��Top=�6T   $	��150��6l   $��18(�6x   �� 5   
   -   5         ��Name$ label��5      $��
Start Date��6   $
��Left=�64   $��	10,10,0,0q�6O   $	��Top=�6b   $��18(�6z   �� 5   
   -   5        $ label1��5      $��End Date��6    $
��Left=�63   $��
205,10,0,0q�6N   $	��Top=�6b   $��18(�6z   �� 5   
   -   5         ��Name$ btnToday��5      $��5,5,5,5q�6#   $��Today��65   $
��Left=�6E   $��
380,42,0,0q�6`   $	��Top=�6t   $��18(�6�   $	��120��6�   �� 5   
   -   5        $ btnLoadReport��5      $��5,5,5,5q�6(   $��View Report��6:   $
��Left=�6P   $��380,124,0,0q�6k   $	��Top=�6�   $��18(�6�   $	��120��6�   �� 5   
   -   5         ��Name$ comboReports��5      $
��Left=�6)   $��
10,124,0,0q�6D   $	��Top=�6X   $	��345��6p   $��18(�6|   $��36��6�   �� 5   
   -	   5         ��Name$ 	txtOutput��5      $��10,173,15,100q�6%    �� TextWrapping$
 Wrap=�6<    �� 
IsReadOnly . $��18(�6b   �� 5   
   -
   5        $ btnPrintReport��5      $��5,5,5,5q�6)   $��Print Report��6;   $
��Left=�6R   $��516,124,0,0q�6m   $	��Top=�6�   $��18(�6�   $	��120��6�   �� 5   
   -   5        $ 
label_Copy��5      $��Select Report��6$   $
��Left=�6<   $��	10,90,0,0q�6W   $	��Top=�6j   $��18(�6�   �� 5   
   -   5        $ btnClose��5      $��5,5,5,5q�6#   $��Close��65   $
��Left=�6E   $��
516,42,0,0q�6`   $	��Top=�6t   $��18(�6�   $	��120��6�   6�   5      5      