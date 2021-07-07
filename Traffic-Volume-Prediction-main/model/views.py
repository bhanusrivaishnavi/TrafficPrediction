from django.shortcuts import redirect, render
import pandas as pd
import numpy as np
import os
import glob
import pyodbc 
import pickle
import matplotlib.pyplot as plt
from sklearn.ensemble import RandomForestRegressor 
from sklearn.metrics import r2_score, mean_squared_error
from sklearn.ensemble import GradientBoostingRegressor
from sklearn.ensemble import AdaBoostRegressor
import math
import re
import math
import nltk
from sklearn.feature_extraction.text import CountVectorizer
nltk.download('stopwords')
from nltk.corpus import stopwords
from nltk.stem.porter import PorterStemmer
from pathlib import Path


req_columns = ['ID', 'Holiday', 'Temp', 'Rain', 'Snow', 'Cloud', 'Weather Main', 'Weather Detail', 'Date Time']


BASE_DIR = Path(__file__).resolve().parent.parent.parent

weather_main_encode = {'Mist': 0, 'Clouds': 1, 'Fog': 2, 'Clear': 3, 'Haze': 4, 'Snow': 5, 'Rain': 6, 'Drizzle': 7, 'Thunderstorm': 8, 'Smoke': 9, 'Squall': 10}
day_encode ={'Wednesday': 0, 'Tuesday': 1, 'Monday': 2, 'Thursday': 3, 'Friday': 4, 'Saturday': 5, 'Sunday': 6}

f = lambda x: weather_main_encode[x]
z = lambda x: False if x == 'None' else True
p = lambda x: day_encode[x]


def prepare_df(df):
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
  df.drop(['Date Time'], axis = 1, inplace = True)
  df['day'] = df['day'].apply(p) 
  df['Temp'] = df['Temp'] - 242
  df['Weather Main'] = df['Weather Main'].apply(f) 
  df.index = np.arange(len(df))
  if 'Public_Or_Private' in df.columns:
    df.drop(['Public_Or_Private'], axis = 1, inplace = True)
  return df
    


    
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

def update_db(conn, filepath):
    cursor = conn.cursor()
    cursor.execute('update FileUploads set IsProcessed=? where FilePath=?;',('true',filepath))



def process(request):
    unprocessed_files = get_file_paths(conn)
        
    for each_file in unprocessed_files:
        test_df = pd.read_csv(each_file)
        req_cols_present = True
        for attr in req_columns:
            if attr not in test_df.columns:
                req_cols_present = False
                break
        if req_cols_present == False:
            continue
        test_df = prepare_df(test_df)
        test_corpus = []
        for i in range(0, len(test_df)):
            weather_detail = re.sub('[^a-zA-Z]', ' ', test_df['Weather Detail'][i])
            weather_detail = weather_detail.lower()
            weather_detail = weather_detail.split()
            ps = PorterStemmer()
            all_stopwords = stopwords.words('english')
            all_stopwords.remove('not')
            weather_detail = [ps.stem(word) for word in weather_detail if not word in set(all_stopwords)]
            weather_detail = ' '.join(weather_detail)
            test_corpus.append(weather_detail)
        vec = pickle.load(open("vector.pickel", "rb"))
        test_df = pd.concat([test_df, pd.DataFrame(vec.transform(test_corpus).toarray())], axis = 1)
        (X_test, Y_test) = (test_df.drop(['Traffic Volume', 'Weather Detail', 'ID', 'Cloud'], axis = 1).values, test_df['Traffic Volume'].values)
        regressor = pickle.load(open("regressor", "rb"))
        Y_pred = regressor.predict(X_test)
     
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

        path = str(BASE_DIR)+ r"\MVCapplication\MVCapplication\wwwroot\Graphs"
        print(path)
        file_name = each_file.split('\\')[-1]
        file_name = file_name.replace(".csv",".png")
        path = os.path.join(path,file_name)
        print("Complete path for PNG file : ",file_name)
        plt.savefig(path)
        update_db(conn, each_file)
        conn.commit()
    
    
    
    return redirect('https://localhost:44300/Home/HomeView')


def homePage(request):


    return render(request, 'model/home.html')






def view_stats(request, file_name):
    path = str(BASE_DIR)+ r"\MVCapplication\MVCapplication\Final"
    print(path)
    path = os.path.join(path,file_name)
    temp = str(BASE_DIR)+ r"\MVCapplication\MVCapplication\Temp"
    path2 = os.path.join(temp, file_name)
    df = pd.read_csv(path)
    df2 = pd.read_csv(path2)
    df2 = prepare_df(df2)
    test_corpus = []
    for i in range(0, len(df2)):
        weather_detail = re.sub('[^a-zA-Z]', ' ', df2['Weather Detail'][i])
        weather_detail = weather_detail.lower()
        weather_detail = weather_detail.split()
        ps = PorterStemmer()
        all_stopwords = stopwords.words('english')
        all_stopwords.remove('not')
        weather_detail = [ps.stem(word) for word in weather_detail if not word in set(all_stopwords)]
        weather_detail = ' '.join(weather_detail)
        test_corpus.append(weather_detail)
    vec = pickle.load(open("vector.pickel", "rb"))
    df2 = pd.concat([df2, pd.DataFrame(vec.transform(test_corpus).toarray())], axis = 1)
    (X_test, Y_test) = (df2.drop(['Traffic Volume', 'Weather Detail', 'ID', 'Cloud'], axis = 1).values, df2['Traffic Volume'].values)
    regressor = pickle.load(open("regressor", "rb"))
    Y_pred = regressor.predict(X_test)


    rmse = round(math.sqrt(mean_squared_error(Y_test, Y_pred)),4)
    cod = round(r2_score(Y_test, Y_pred),4)



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
        "file_name" : file_name,
        "rmse" : rmse,
        "cod" : cod,

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



# from django.shortcuts import redirect, render
# import pandas as pd
# import numpy as np
# import os
# import glob
# import pyodbc 
# import pickle
# import matplotlib.pyplot as plt
# from sklearn.ensemble import RandomForestRegressor 
# from sklearn.metrics import r2_score, mean_squared_error
# from sklearn.ensemble import GradientBoostingRegressor
# from sklearn.ensemble import AdaBoostRegressor
# import math
# import re
# import math
# import nltk
# from sklearn.feature_extraction.text import CountVectorizer
# nltk.download('stopwords')
# from nltk.corpus import stopwords
# from nltk.stem.porter import PorterStemmer
# from pathlib import Path


# req_columns = ['ID', 'Holiday', 'Temp', 'Rain', 'Snow', 'Cloud', 'Weather Main', 'Weather Detail', 'Date Time']


# BASE_DIR = Path(__file__).resolve().parent.parent.parent

# weather_main_encode = {'Mist': 0, 'Clouds': 1, 'Fog': 2, 'Clear': 3, 'Haze': 4, 'Snow': 5, 'Rain': 6, 'Drizzle': 7, 'Thunderstorm': 8, 'Smoke': 9, 'Squall': 10}
# day_encode ={'Wednesday': 0, 'Tuesday': 1, 'Monday': 2, 'Thursday': 3, 'Friday': 4, 'Saturday': 5, 'Sunday': 6}

# f = lambda x: weather_main_encode[x]
# z = lambda x: False if x == 'None' else True
# p = lambda x: day_encode[x]


# def prepare_df(df):
#   df['Date Time'] = pd.to_datetime(df['Date Time'])
#   df['time'] = df['Date Time'].dt.hour
#   df['Holiday'] = df['Holiday'].apply(z)
#   df['year'] = df['Date Time'].dt.year
#   df['month'] = df['Date Time'].dt.month
#   df['day'] = df['Date Time'].dt.day_name()
#   df = df[df['Temp'] != 0]
#   df.index = np.arange(len(df))
#   df = df[df.Rain < 100]
#   df.index = np.arange(len(df))
#   df.drop(['Date Time'], axis = 1, inplace = True)
#   df['day'] = df['day'].apply(p) 
#   df['Temp'] = df['Temp'] - 242
#   df['Weather Main'] = df['Weather Main'].apply(f) 
#   df.index = np.arange(len(df))
#   if 'Public_Or_Private' in df.columns:
#     df.drop(['Public_Or_Private'], axis = 1, inplace = True)
#   return df
    


    
# # Create your views here.
# conn = pyodbc.connect('Driver={SQL Server Native Client 11.0};'
#                       'Server=DESKTOP-JUH932N\\SQLEXPRESS;'
#                       'Database=user;'
#                       'Trusted_Connection=yes;')

# def get_file_paths(conn):
#     cursor = conn.cursor()
#     cursor.execute('SELECT FilePath FROM FileUploads where IsProcessed=?;',('false'))
#     print('-----------------------> Read the files successfully!!!!!!!')

#     li = []

#     for row in cursor:
#         li.append(row[0])

#     print("List of unprocessed files...................................")

#     for i in range(len(li)):
#         print(li[i].split("\\")[-1])
#     print('............................................................')

#     return li

# def update_db(conn, filepath):
#     cursor = conn.cursor()
#     cursor.execute('update FileUploads set IsProcessed=? where FilePath=?;',('true',filepath))



# def process(request):
#     unprocessed_files = get_file_paths(conn)
        
#     for each_file in unprocessed_files:
#         test_df = pd.read_csv(each_file)
#         req_cols_present = True
#         for attr in req_columns:
#             if attr not in test_df.columns:
#                 req_cols_present = False
#                 break
#         if req_cols_present == False:
#             continue
#         test_df = prepare_df(test_df)
#         test_corpus = []
#         for i in range(0, len(test_df)):
#             weather_detail = re.sub('[^a-zA-Z]', ' ', test_df['Weather Detail'][i])
#             weather_detail = weather_detail.lower()
#             weather_detail = weather_detail.split()
#             ps = PorterStemmer()
#             all_stopwords = stopwords.words('english')
#             all_stopwords.remove('not')
#             weather_detail = [ps.stem(word) for word in weather_detail if not word in set(all_stopwords)]
#             weather_detail = ' '.join(weather_detail)
#             test_corpus.append(weather_detail)
       
#         vec = pickle.load(open("vector.pickel", "rb"))
#         test_df = pd.concat([test_df, pd.DataFrame(vec.transform(test_corpus).toarray())], axis = 1)
#         (X_test, Y_test) = (test_df.drop(['Traffic Volume', 'Weather Detail', 'ID', 'Cloud'], axis = 1).values, test_df['Traffic Volume'].values)
        
#         regressor = pickle.load(open("regressor", "rb"))
#         Y_pred = regressor.predict(X_test)
     
#         for i in range(len(Y_pred)):
#             Y_pred[i] = round(Y_pred[i],0)
#         data = {'ID': test_df['ID'],'Traffic Volume': Y_pred}
#         new_df = pd.DataFrame(data, columns= ['ID', 'Traffic Volume'])
#         new_df = new_df.sort_values(by=['ID'])

#         storage_path = each_file
#         storage_path = storage_path.replace("Temp","Final")
#         new_df.to_csv(storage_path, index = False)

#         max_value = new_df['Traffic Volume'].max()
#         min_value = new_df['Traffic Volume'].min()
#         diff = (max_value - min_value + 1)/5
#         ranges = []
#         for i in range(5):
#             ranges.append(round((i+1)*diff,0))
#             print(ranges[i])
#         freqs = [0,0,0,0,0]
#         for i in range(len(new_df)):
#             for j in range(5):
#                 if new_df['Traffic Volume'][i] <= ranges[j]:
#                     freqs[j] += 1
#                     break
#         print('ranges : ',ranges)
#         print('freqs : ', freqs)
#         labels = []
#         for i in range(len(ranges)):
#             if i==0:
#                 val = str(min_value) + ' - ' + str(ranges[i])
#             else:
#                 val = str(ranges[i-1]+1) + ' - ' + str(ranges[i])
#             labels.append(val)
#         print('labels : ', labels)

#         fig = plt.figure(figsize = (8, 5))
#         plt.bar(labels, freqs, color ='blue',width = 0.4)
#         plt.xlabel("Traffic Volume Range")
#         plt.ylabel("No. of rows within the range")
#         plt.title("Traffic Volume Prediction")

#         path = str(BASE_DIR)+ r"\MVCapplication\MVCapplication\wwwroot\Graphs"
#         print(path)
#         file_name = each_file.split('\\')[-1]
#         file_name = file_name.replace(".csv",".png")
#         path = os.path.join(path,file_name)
#         print("Complete path for PNG file : ",file_name)
#         plt.savefig(path)
#         update_db(conn, each_file)
#         conn.commit()
    
    
    
#     return redirect('https://localhost:44300/Home/HomeView')


# def homePage(request):


#     return render(request, 'model/home.html')






# def view_stats(request, file_name):
#     print(file_name)
#     path = str(BASE_DIR)+ r"\MVCapplication\MVCapplication\Final"
#     print(path)
#     path = os.path.join(path,file_name)
#     df = pd.read_csv(path)
#     max_value = df['Traffic Volume'].max()
#     min_value = df['Traffic Volume'].min()
#     print('max : ',max_value)
#     print('min : ',min_value)
#     diff = (max_value - min_value + 1)/5
#     print('diff : ',diff)
#     ranges = []
#     for i in range(5):
#         ranges.append(round((i+1)*diff,0))
#         print(ranges[i])
#     freqs = [0,0,0,0,0]
#     for i in range(len(df)):
#         for j in range(5):
#             if df['Traffic Volume'][i] <= ranges[j]:
#                 freqs[j] += 1
#                 break
#     print('ranges : ',ranges)
#     print('freqs : ', freqs)
#     labels = []
#     for i in range(len(ranges)):
#         if i==0:
#             val = str(min_value) + ' - ' + str(ranges[i])
#         else:
#             val = str(ranges[i-1]+1) + ' - ' + str(ranges[i])
#         labels.append(val)
#     print('labels : ', labels)

#     file_names = file_name.split('_')
#     file_name = file_names[1]+'_'+file_names[2]


#     context = {
#         "path" : path,
#         "labels" : labels,
#         "freqs" : freqs,
#         "file_name" : file_name

#     }
    
#     return render(request, 'model/stats.html',context)












    





#df=pd.read_csv(r'C:\Users\Vaishnavi\Downloads\Final_Traffic_Data\Final_Traffic_Data\TrafficVolume_Train.csv')
 # train_df=prepare_train(df)
  #  (X_train,Y_train)=(train_df.drop(['Traffic Volume','ID','Cloud'],axis=1).values,train_df['Traffic Volume'].values)
   # X_train=scaler.fit_transform(X_train)
#rfreg=RandomForestRegressor(n_estimators=50,max_depth=12,min_samples_split=5)
 #       rfreg.fit(X_train,Y_train)

# with open('model_pickle','wb') as f:
 #           pickle.dump(rfreg,f)