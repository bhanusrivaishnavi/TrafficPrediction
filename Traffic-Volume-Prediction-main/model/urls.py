from django.urls import path
from . import views


urlpatterns = [
    path('', views.homePage, name="home"),
    path('process/', views.process, name="process"),
    path('view_stats/<str:file_name>/', views.view_stats, name='view_stats'),

    
]
