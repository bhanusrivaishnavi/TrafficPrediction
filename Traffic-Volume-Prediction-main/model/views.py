from django.shortcuts import redirect, render
import pandas as pd
import numpy as np
import os
import glob
import pyodbc 
import pickle
import matplotlib.pyplot as plt
from nltk.sentiment import SentimentIntensityAnalyzer
from sklearn.preprocessing import LabelEncoder
from sklearn.preprocessing import StandardScaler
from sklearn.linear_model import LinearRegression
from sklearn.tree import DecisionTreeRegressor
from sklearn.ensemble import RandomForestRegressor 
from sklearn.metrics import r2_score, mean_squared_error
from sklearn.ensemble import GradientBoostingRegressor
from sklearn.ensemble import AdaBoostRegressor
import math
import matplotlib.pyplot as plt


sia = SentimentIntensityAnalyzer()

def nlp_wd(text):
  return sia.polarity_scores(text)['pos']

z = lambda x: False if x == 'None' else True


def prepare_train(df):
  df['Date Time'] = pd.to_datetime(df['Date Time'])
  df['time'] = df['Date Time'].dt.hour
  df['Holiday'] = df['Holiday'].apply(z)
  df['year'] = df['Date Time'].dt.year
  df['month'] = df['Date Time'].dt.month
  df['day'] = df['Date Time'].dt.day_name()
  df = df[df['Temp'] != 0]
  df.index = np.arange(len(df))
  df = df[df.Rain < 100]
  df.index = np.arange(len(df))
  df['Weather Detail'] = df['Weather Detail'].apply(nlp_wd)
  df.drop(['Date Time'], axis = 1, inplace = True)
  encoder = LabelEncoder()
  df['day'] = encoder.fit_transform(df['day'])
  df['Temp'] = df['Temp'] - 242
  encoder = LabelEncoder()
  df['Weather Main'] = encoder.fit_transform(df['Weather Main'])
  df.index = np.arange(len(df))
  return df
    

def prepare_test(df):
  test_df = df
  test_df['Date Time'] = pd.to_datetime(test_df['Date Time'])
  test_df['time'] = test_df['Date Time'].dt.hour
  z = lambda x: False if x == 'None' else True
  test_df['Holiday'] = test_df['Holiday'].apply(z)
  test_df['year'] = test_df['Date Time'].dt.year
  test_df['month'] = test_df['Date Time'].dt.month
  test_df['day'] = test_df['Date Time'].dt.day_name()
  test_df = test_df[test_df['Temp'] != 0]
  test_df.index = np.arange(len(test_df))
  test_df = test_df[test_df.Rain < 100]
  test_df.index = np.arange(len(test_df))
  test_df['Weather Detail'] = test_df['Weather Detail'].apply(nlp_wd)
  test_df.drop(['Date Time'], axis = 1, inplace = True)
  encoder = LabelEncoder()
  test_df['day'] = encoder.fit_transform(test_df['day'])
  test_df['Temp'] = test_df['Temp'] - 242
  encoder = LabelEncoder()
  test_df['Weather Main'] = encoder.fit_transform(test_df['Weather Main'])
  test_df.index = np.arange(len(test_df))
  if 'Public_Or_Private' in test_df.columns:
    test_df.drop(['Public_Or_Private'], axis = 1, inplace = True)
  return test_df
    
# Create your views here.
conn = pyodbc.connect('Driver={SQL Server Native Client 11.0};'
                      'Server=DESKTOP-JUH932N\\SQLEXPRESS;'
                      'Database=user;'
                      'Trusted_Connection=yes;')

def get_file_paths(conn):
    cursor = conn.cursor()
    cursor.execute('SELECT FilePath FROM FileUploads where IsProcessed=?;',('false'))
    print('-----------------------> Read the files successfully!!!!!!!')

    li = []

    for row in cursor:
        li.append(row[0])

    print("List of unprocessed files...................................")

    for i in range(len(li)):
        print(li[i].split("\\")[-1])
    print('............................................................')

    return li

def update_db(conn):
    cursor = conn.cursor()
    cursor.execute('update FileUploads set IsProcessed=?;',('true'))



def process(request):
    unprocessed_files = get_file_paths(conn)
    scaler = StandardScaler()
    with open('model_pickle','rb') as f:
        mp=pickle.load(f)

        
    for each_file in unprocessed_files:
        test_df = pd.read_csv(each_file)
        test_df = prepare_test(test_df)
        (X_test, Y_test) = (test_df.drop(['Traffic Volume', 'ID', 'Cloud'], axis = 1).values, test_df['Traffic Volume'].values)
        X_test = scaler.fit_transform(X_test)
        
       
        Y_pred = mp.predict(X_test)
        for i in range(len(Y_pred)):
            Y_pred[i] = round(Y_pred[i],0)
        data = {'ID': test_df['ID'],'Traffic Volume': Y_pred}
        new_df = pd.DataFrame(data, columns= ['ID', 'Traffic Volume'])
        new_df = new_df.sort_values(by=['ID'])

        storage_path = each_file
        storage_path = storage_path.replace("Temp","Final")
        new_df.to_csv(storage_path, index = False)

        max_value = new_df['Traffic Volume'].max()
        min_value = new_df['Traffic Volume'].min()
        diff = (max_value - min_value + 1)/5
        ranges = []
        for i in range(5):
            ranges.append(round((i+1)*diff,0))
            print(ranges[i])
        freqs = [0,0,0,0,0]
        for i in range(len(new_df)):
            for j in range(5):
                if new_df['Traffic Volume'][i] <= ranges[j]:
                    freqs[j] += 1
                    break
        print('ranges : ',ranges)
        print('freqs : ', freqs)
        labels = []
        for i in range(len(ranges)):
            if i==0:
                val = str(min_value) + ' - ' + str(ranges[i])
            else:
                val = str(ranges[i-1]+1) + ' - ' + str(ranges[i])
            labels.append(val)
        print('labels : ', labels)

        fig = plt.figure(figsize = (8, 5))
        plt.bar(labels, freqs, color ='blue',width = 0.4)
        plt.xlabel("Traffic Volume Range")
        plt.ylabel("No. of rows within the range")
        plt.title("Traffic Volume Prediction")
        path = r'C:\Users\Vaishnavi\source\repos\TrafficPrediction\MVCapplication\MVCapplication\wwwroot\Graphs'
        file_name = each_file.split('\\')[-1]
        file_name = file_name.replace(".csv",".png")
        path = os.path.join(path,file_name)
        print("Complete path for PNG file : ",file_name)
        plt.savefig(path)
    
    update_db(conn)
    conn.commit()

    return redirect('https://localhost:44300/Home/HomeView')
    


def homePage(request):


    return render(request, 'model/home.html')






def view_stats(request, file_name):
    path = r'C:\Users\Vaishnavi\source\repos\TrafficPrediction\MVCapplication\MVCapplication\Final'
    path = os.path.join(path,file_name)
    df = pd.read_csv(path)
    max_value = df['Traffic Volume'].max()
    min_value = df['Traffic Volume'].min()
    print('max : ',max_value)
    print('min : ',min_value)
    diff = (max_value - min_value + 1)/5
    print('diff : ',diff)
    ranges = []
    for i in range(5):
        ranges.append(round((i+1)*diff,0))
        print(ranges[i])
    freqs = [0,0,0,0,0]
    for i in range(len(df)):
        for j in range(5):
            if df['Traffic Volume'][i] <= ranges[j]:
                freqs[j] += 1
                break
    print('ranges : ',ranges)
    print('freqs : ', freqs)
    labels = []
    for i in range(len(ranges)):
        if i==0:
            val = str(min_value) + ' - ' + str(ranges[i])
        else:
            val = str(ranges[i-1]+1) + ' - ' + str(ranges[i])
        labels.append(val)
    print('labels : ', labels)

    file_names = file_name.split('_')
    file_name = file_names[1]+'_'+file_names[2]


    context = {
        "path" : path,
        "labels" : labels,
        "freqs" : freqs,
        "file_name" : file_name

    }
    
    return render(request, 'model/stats.html',context)












    





#df=pd.read_csv(r'C:\Users\Vaishnavi\Downloads\Final_Traffic_Data\Final_Traffic_Data\TrafficVolume_Train.csv')
 # train_df=prepare_train(df)
  #  (X_train,Y_train)=(train_df.drop(['Traffic Volume','ID','Cloud'],axis=1).values,train_df['Traffic Volume'].values)
   # X_train=scaler.fit_transform(X_train)
#rfreg=RandomForestRegressor(n_estimators=50,max_depth=12,min_samples_split=5)
 #       rfreg.fit(X_train,Y_train)

# with open('model_pickle','wb') as f:
 #           pickle.dump(rfreg,f)