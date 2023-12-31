<?xml version="1.0" encoding="utf-8"?>
<Grid
  xmlns="www.team-mediaportal.com/2008/mpf/directx"
  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
  xmlns:fanart="clr-namespace:MediaPortal.Extensions.UserServices.FanArtService.Client;assembly=FanArtService.Client"
  xmlns:aspect="clr-namespace:MediaPortal.Common.MediaManagement.DefaultItemAspects;assembly=MediaPortal.Common"
  xmlns:media="clr-namespace:MediaPortal.UiComponents.Media.Models.AspectWrappers;assembly=Media"
  xmlns:mp_special_controls="clr-namespace:MediaPortal.UI.SkinEngine.SpecialElements.Controls;assembly=SkinEngine"
  xmlns:media_consts="clr-namespace:MediaPortal.UiComponents.Media.General;assembly=Media"
  x:Name="MediaItemDetails" Margin="30,10,20,10"
  HorizontalAlignment="Stretch" VerticalAlignment="Stretch">

  <Grid.Resources>
    <fanart:FanArtImageSourceConverter x:Key="FanArtImageSourceConverter"/>
    <MultiAspectAttributeConverter x:Key="MultiAspectAttributeConverter"/>
  </Grid.Resources>
  <Grid.RowDefinitions>
    <RowDefinition Height="Auto"/>
    <RowDefinition Height="*"/>
  </Grid.RowDefinitions>
  <Grid.ColumnDefinitions>
    <ColumnDefinition Width="600"/>
    <ColumnDefinition Width="*"/>
  </Grid.ColumnDefinitions>

  <Grid HorizontalAlignment="Stretch" VerticalAlignment="Bottom" Grid.Row="1" Grid.Column="1" Margin="-250,-150,20,30">
    <Image x:Name="CDImage" Stretch="Fill"
           FallbackSource="CD.png" HorizontalAlignment="Right" RenderTransformOrigin="0.498,0.498"
           VerticalAlignment="Center" Width="380" Height="380" Margin="-40,0,190,20">
      <Image.Source>
        <fanart:FanArtImageSource fanart:FanArtMediaType="Album" fanart:FanArtType="DiscArt" fanart:FanArtName="{Binding MediaItem.MediaItemId}"
                                              fanart:MaxWidth="380" fanart:MaxHeight="380"/>
      </Image.Source>
      <Image.Resources>
        <Storyboard x:Key="DiskRotationStoryboard" FillBehavior="HoldEnd" RepeatBehavior="Forever">
          <DoubleAnimation Storyboard.TargetProperty="RenderTransform.Angle"
                           From="0" To="360" Duration="0:0:4.5" RepeatBehavior="Forever" />
         </Storyboard>
        </Image.Resources>
        <Image.Triggers>
        <DataTrigger Binding="{Binding IsVisible}" Value="True">
          <DataTrigger.EnterActions>
            <BeginStoryboard x:Name="Disabled_BeginStoryBoard" Storyboard="{ThemeResource DiskRotationStoryboard}"/>
          </DataTrigger.EnterActions>
          <DataTrigger.ExitActions>
            <StopStoryboard BeginStoryboardName="Disabled_BeginStoryBoard"/>
          </DataTrigger.ExitActions>
        </DataTrigger>
      </Image.Triggers>
      <Image.RenderTransform>
        <RotateTransform Angle="0" />
      </Image.RenderTransform>
    </Image>

    <Grid HorizontalAlignment="Right" VerticalAlignment="Center" Width="400" Height="400" Margin="0,-20,0,0" Background="{ThemeResource NereusColorMedium}">               
      <Image Stretch="Uniform" Source="audio.png" HorizontalAlignment="Center" VerticalAlignment="Center" 
             IsVisible="{Binding ElementName=Thumb, Path=!HasImage}" Margin="50"/>
      <Image x:Name="Thumb" Stretch="Fill" Source="{Binding MediaItem}" HorizontalAlignment="Stretch"
                         VerticalAlignment="Stretch" Width="400" Height="400"/>
      <Image Stretch="Fill" HorizontalAlignment="Stretch" VerticalAlignment="Stretch" Width="400" Height="400">
        <Image.Source>
        <fanart:FanArtImageSource fanart:FanArtMediaType="Album" fanart:FanArtType="Poster" fanart:FanArtName="{Binding MediaItem.MediaItemId}"
                                    fanart:MaxWidth="400" fanart:MaxHeight="400"/>
        </Image.Source>
      </Image>
    </Grid>
  </Grid>

  <Image Grid.Row="1" Grid.Column="1" Margin="-200,-230,10,0" HorizontalAlignment="Center" VerticalAlignment="Top" Stretch="Uniform" Height="140">
    <Image.Source>
      <fanart:FanArtImageSource fanart:FanArtMediaType="Audio" fanart:FanArtType="ClearArt" fanart:FanArtName="{Binding MediaItem.MediaItemId}"
                                fanart:MaxWidth="250" fanart:MaxHeight="120"/>
    </Image.Source>
  </Image>

  <media:AudioAspectWrapper MediaItem="{Binding MediaItem}" >
    <media:AudioAspectWrapper.Template>
      <ControlTemplate>
        <StackPanel Orientation="Vertical" DataContext="{Binding RelativeSource={RelativeSource TemplatedParent}}" Margin="0,0,20,0">

          <Label x:Name="Title" FontSize="{ThemeResource NormalFontSize}" Wrap="True" TextTrimming="WordEllipsis" Margin="0,0,-150,0"
                 Content="{Binding TrackName}" Color="{ThemeResource TextColor}" FontFamily="SegoeUIBold"/>

          <Label x:Name="Artists" FontSize="{ThemeResource SmallFontSize}"
                 Content="{Binding Artists}" Color="{ThemeResource TextColor}"/>

          <StackPanel Orientation="Horizontal" HorizontalAlignment="Left" VerticalAlignment="Center" Margin="0,3,0,3"
                      IsVisible="{Binding Path=TotalRating,Converter={StaticResource ReferenceToBoolConverter}}">
            <StarRatingPanel x:Name="RatingReadOnly" IsReadOnly="True" StarReadOnlyTemplate="{StaticResource StarReadOnlyTemplate}"
                             Maximum="5" VerticalAlignment="Center">
              <StarRatingPanel.Value>
                <MultiBinding Converter="{StaticResource ExpressionMultiValueConverter}" ConverterParameter="{}{0}/2">
                  <Binding Path="TotalRating"/>
                </MultiBinding>
              </StarRatingPanel.Value>
              <StarRatingPanel.LayoutTransform>
                <TransformGroup>
                  <ScaleTransform ScaleX="1.6" ScaleY="1.5"/>
                </TransformGroup>
              </StarRatingPanel.LayoutTransform>
            </StarRatingPanel>
            <Label x:Name="RatingCountLabel" Color="{ThemeResource TextColorForeground}" VerticalAlignment="Center"
                   FontSize="{ThemeResource SmallerFontSize}" Margin="15,0,5,1"
                   IsVisible="{Binding Path=TotalRating,Converter={StaticResource ReferenceToBoolConverter}}">
              <Label.Content>
                <Binding Path="TotalRating" Converter="{StaticResource StringFormatConverter}" ConverterParameter="{}({0:F1})"/>
              </Label.Content>
            </Label>
          </StackPanel>

          <Grid Margin="0,0,20,0" IsVisible="{Binding ElementName=Album, Path=Content, Converter={StaticResource EmptyStringToFalseConverter}}">
            <Image Source="HomeContentIcons\album.png" Height="26" Width="26" Stretch="Uniform" Margin="2,-1,5,1" 
                   VerticalAlignment="Center" HorizontalAlignment="Left"/>
            <Label x:Name="Album" Grid.Row="0" Grid.Column="1" FontSize="{ThemeResource SmallFontSize}"
                   Content="{Binding Album}" Color="{ThemeResource TextColorForeground}" Margin="40,0,0,0"/>
          </Grid>

          <Grid DataContext="{Binding RelativeSource={RelativeSource TemplatedParent}}" Margin="0,0,20,0"
                IsVisible="{Binding ElementName=AlbumArtists, Path=Content, Converter={StaticResource EmptyStringToFalseConverter}}">
            <Image Source="HomeContentIcons\person.png" Height="30" Width="30" Stretch="Uniform" Margin="0,0,5,0"
                   VerticalAlignment="Center" HorizontalAlignment="Left"/>
            <Label x:Name="AlbumArtists" FontSize="{ThemeResource SmallFontSize}" VerticalAlignment="Center"
                   Content="{Binding Path=AlbumArtists}" Color="{ThemeResource TextColorForeground}" Margin="40,0,0,0"/>
          </Grid>

          <Grid IsVisible="{Binding ElementName=Genre, Path=Content, Converter={StaticResource EmptyStringToFalseConverter}}">
            <Image Source="HomeContentIcons\audiogenre.png" Height="30" Width="30" Stretch="Uniform" Margin="0,0,5,0" 
                   VerticalAlignment="Center" HorizontalAlignment="Left"/>
            <Label x:Name="Genre" FontSize="{ThemeResource SmallFontSize}" TextTrimming="WordEllipsis" 
                   Color="{ThemeResource TextColorForeground}" Margin="40,0,0,0">
              <Label.Content>
                <MultiBinding Converter="{StaticResource MultiAspectAttributeConverter}">
                  <Binding Path="MediaItem"/>
                  <Binding Source="{x:Static aspect:GenreAspect.ATTR_GENRE}" />
                </MultiBinding>
              </Label.Content>
            </Label>
          </Grid>

          <media:UserDataWrapper MediaItem="{Binding MediaItem}">
            <media:UserDataWrapper.Template>
              <ControlTemplate>
                <Grid DataContext="{Binding RelativeSource={RelativeSource TemplatedParent}}"
                      IsVisible="{Binding Path=PlayPercentage,Converter={StaticResource ReferenceToBoolConverter}}">
                  <Image VerticalAlignment="Center" Stretch="Fill" Width="95" Height="95" Margin="0,10,40,0" IsVisible="{Binding Source={StaticResource SkinSettingsModel}, Path=[Nereus].UseTorquoise}">
                    <Image.Source>
                      <Binding Path="PlayPercentage" Converter="{StaticResource RoundingFormatConverter}" ConverterParameter="{}5;CircularBar\{0}_torquoise.png"/>
                    </Image.Source>
                  </Image>
                  <Image VerticalAlignment="Center" Stretch="Fill" Width="95" Height="95" Margin="0,10,40,0" IsVisible="{Binding Source={StaticResource SkinSettingsModel}, Path=[Nereus].UseYellow}">
                    <Image.Source>
                      <Binding Path="PlayPercentage" Converter="{StaticResource RoundingFormatConverter}" ConverterParameter="{}5;CircularBar\{0}_yellow.png"/>
                    </Image.Source>
                  </Image>
                  <Image VerticalAlignment="Center" Stretch="Fill" Width="95" Height="95" Margin="0,10,40,0" IsVisible="{Binding Source={StaticResource SkinSettingsModel}, Path=[Nereus].UseOrange}">
                    <Image.Source>
                      <Binding Path="PlayPercentage" Converter="{StaticResource RoundingFormatConverter}" ConverterParameter="{}5;CircularBar\{0}_orange.png"/>
                    </Image.Source>
                  </Image>
                  <Image VerticalAlignment="Center" Stretch="Fill" Width="95" Height="95" Margin="0,10,40,0" IsVisible="{Binding Source={StaticResource SkinSettingsModel}, Path=[Nereus].UseRed}">
                    <Image.Source>
                      <Binding Path="PlayPercentage" Converter="{StaticResource RoundingFormatConverter}" ConverterParameter="{}5;CircularBar\{0}_red.png"/>
                    </Image.Source>
                  </Image>
                  <Image VerticalAlignment="Center" Stretch="Fill" Width="95" Height="95" Margin="0,10,40,0" IsVisible="{Binding Source={StaticResource SkinSettingsModel}, Path=[Nereus].UsePurple}">
                    <Image.Source>
                      <Binding Path="PlayPercentage" Converter="{StaticResource RoundingFormatConverter}" ConverterParameter="{}5;CircularBar\{0}_purple.png"/>
                    </Image.Source>
                  </Image>
                  <Image VerticalAlignment="Center" Stretch="Fill" Width="95" Height="95" Margin="0,10,40,0" IsVisible="{Binding Source={StaticResource SkinSettingsModel}, Path=[Nereus].UseGreen}">
                    <Image.Source>
                      <Binding Path="PlayPercentage" Converter="{StaticResource RoundingFormatConverter}" ConverterParameter="{}5;CircularBar\{0}_green.png"/>
                    </Image.Source>
                  </Image>
                  <Image VerticalAlignment="Center" Stretch="Fill" Width="95" Height="95" Margin="0,10,40,0" IsVisible="{Binding Source={StaticResource SkinSettingsModel}, Path=[Nereus].UseBlue}">
                    <Image.Source>
                      <Binding Path="PlayPercentage" Converter="{StaticResource RoundingFormatConverter}" ConverterParameter="{}5;CircularBar\{0}_blue.png"/>
                    </Image.Source>
                  </Image>
                  <Image VerticalAlignment="Center" Stretch="Fill" Width="95" Height="95" Margin="0,10,40,0" IsVisible="{Binding Source={StaticResource SkinSettingsModel}, Path=[Nereus].UseGrey}">
                    <Image.Source>
                      <Binding Path="PlayPercentage" Converter="{StaticResource RoundingFormatConverter}" ConverterParameter="{}5;CircularBar\{0}_grey.png"/>
                    </Image.Source>
                  </Image>
                  <Label VerticalAlignment="Center" HorizontalAlignment="Center" FontSize="20" Color="White" FontFamily="SegoeUIBold"
                         Width="95" Height="95" Margin="0,0,40,0" HorizontalContentAlignment="Center" VerticalContentAlignment="Center">
                    <Label.Content>
                      <Binding Path="PlayPercentage" Converter="{StaticResource StringFormatConverter}" ConverterParameter="{}{0:F0}%"/>
                    </Label.Content>
                  </Label>
                </Grid>
              </ControlTemplate>
            </media:UserDataWrapper.Template>
          </media:UserDataWrapper>

          <media:MediaAspectWrapper MediaItem="{Binding MediaItem}" Margin="0,30,20,0">
            <media:MediaAspectWrapper.Template>
              <ControlTemplate>
                <Grid DataContext="{Binding RelativeSource={RelativeSource TemplatedParent}}"
                      IsVisible="{Binding ElementName=Year, Path=Content, Converter={StaticResource EmptyStringToFalseConverter}}">
                  <Image Source="HomeContentIcons\year.png" Height="27" Width="27" Stretch="Uniform" Margin="0,-1,5,1" 
                         VerticalAlignment="Center" HorizontalAlignment="Left"/>
                  <Label x:Name="Year" FontSize="{ThemeResource SmallFontSize}" VerticalAlignment="Center" Color="{ThemeResource TextColorForeground}"
                         Content="{Binding Path=RecordingTime, Converter={StaticResource DateFormatConverter}, ConverterParameter=yyyy}" Margin="40,0,0,0"/>
                </Grid>
              </ControlTemplate>
            </media:MediaAspectWrapper.Template>
          </media:MediaAspectWrapper>

          <Grid Margin="0,0,20,0" IsVisible="{Binding ElementName=TrackLabel, Path=Content, Converter={StaticResource EmptyStringToFalseConverter}}">
            <Image Source="HomeContentIcons\track.png" Height="29" Width="28" Stretch="Uniform" Margin="0,0,5,0"
                   VerticalAlignment="Center" HorizontalAlignment="Left"/>
            <Label x:Name="TrackLabel" Content="{Binding Track}" Color="{ThemeResource TextColorForeground}" 
                     VerticalAlignment="Center" FontSize="{ThemeResource SmallFontSize}" Margin="40,0,0,0"/>
          </Grid>

          <Grid IsVisible="{Binding ElementName=Duration, Path=Content, Converter={StaticResource EmptyStringToFalseConverter}}">
            <Image Source="HomeContentIcons\duration.png" Height="26" Width="26" Stretch="Uniform" Margin="2,-1,5,1"
                   VerticalAlignment="Center" HorizontalAlignment="Left"/>
            <Label x:Name="Duration" FontSize="{ThemeResource SmallFontSize}" Color="{ThemeResource TextColorForeground}" Margin="40,0,0,0"
                   Content="{Binding Path=Duration, Converter={StaticResource DurationConverter}, ConverterParameter=mm\:ss}"/>
          </Grid>

          <Grid Margin="0,0,20,0" IsVisible="{Binding ElementName=NumDiscs, Path=Content, Converter={StaticResource EmptyStringToFalseConverter}}">
            <Image Source="HomeContentIcons\disc.png" Height="28" Width="28" Stretch="Uniform" Margin="2,-1,5,1"
                   VerticalAlignment="Center" HorizontalAlignment="Left"/>
            <StackPanel Orientation="Horizontal" VerticalAlignment="Center" Margin="40,0,0,0">
              <Label FontSize="{ThemeResource SmallFontSize}" Content="{Binding Path=DiscId}" Color="{ThemeResource TextColorForeground}" VerticalAlignment="Center"/>
              <Label FontSize="{ThemeResource SmallerFontSize}" Content=" | " Color="{ThemeResource TextColorForeground}" VerticalAlignment="Center"/>
              <Label x:Name="NumDiscs" FontSize="{ThemeResource SmallFontSize}" Content="{Binding Path=NumDiscs}" 
                     Color="{ThemeResource TextColorForeground}" VerticalAlignment="Center"/>
            </StackPanel>
          </Grid>

        </StackPanel>
      </ControlTemplate>
    </media:AudioAspectWrapper.Template>
  </media:AudioAspectWrapper>

  <Button x:Name="PlayButton" Grid.Column="1" Margin="0,-60,50,0" Command="{Command Play}" Style="{ThemeResource EmptyButtonStyle}"
          HorizontalAlignment="Right" VerticalAlignment="Top" Width="190" Height="190">
    <Grid>
      <Image Stretch="Uniform" Width="190" Height="190"
             Source="playback.png" Opacity="0.8" IsVisible="{Binding RelativeSource={RelativeSource Mode=FindAncestor,AncestorType={x:Type Button}},Path=!HasFocus}" />
      <Image Stretch="Uniform" Width="190" Height="190"
             Source="playback_focus.png" Opacity="1" IsVisible="{Binding RelativeSource={RelativeSource Mode=FindAncestor,AncestorType={x:Type Button}},Path=HasFocus}" />
    </Grid>
  </Button>

</Grid>
