   M S B A M L   `   `   `  ���� A  =Reader, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullclr-namespace:ReaderReader      Reader.MainWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     &localclr-namespace:Reader       � Title$  Mango��$	��480��$	��940�� � WindowStyle$ ThreeDBorderWindow=� � 	Resources    MangaListTemplate �� 
TargetType��%   &	        �� " ��.R�� .��� ���� *ActualWidth+ �� RelativeSource ��  �� AncestorType" ����� *ActualHeight+ �� " �.+�c� $��26��$	��Top=�.@�o� .@�m� $��Mango��.@�m� $��Themes��.@�m� $��NIGGA��� $��
0,26,25,45q� � ColumnDefinitions �� $
��142* ��� $
��495* �.+��� -   $��	MangaList��$��0��
�������$��10,10,10,10q�����  �� 
TargetType" ��.�-   ���� .R�� $	��0,2q�$��Right=� �� $��* ��� $	��100 �.+��� ���� *Title+� $��1�� � Minimum$ 0Q�	 � Maximum$		 100Q�
 � Value
 �� *
Completion+� $��1��$��0,10,10,150q� � RowDefinitions �� $
��385* ��� $��* �.+�� $��0,0,0,0q�.+��� -    ��Name$ MangoThumbnail��$	��Top=�$
��Left=�$	��300��$
��auto���� $	��352��$��	-3,0,0,-3q�$
��Left=�$��Stretch=�$��/res/gradiant.png��$
��None=� �� StretchDirection$ DownOnly=��� -    ��Name$ MangoSynopsisBorder��
�������
�������$��1q� �� CornerRadius$ 8,8,3,3��$��Stretch=�$��0,0,0,0q�._��� -    ��Name$ MangoSynopsisText��$
��Wrap=�$��sa��$	��Top=�$
��Left=�$
��Auto��$
��Auto��$��5,5,0,0q�