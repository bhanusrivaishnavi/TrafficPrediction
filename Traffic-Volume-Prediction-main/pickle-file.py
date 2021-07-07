import pandas as pd
import pickle
import numpy as np
import re
import math
import nltk
from sklearn.feature_extraction.text import CountVectorizer
from sklearn.tree import DecisionTreeRegressor
from sklearn.ensemble import RandomForestRegressor 
from sklearn.metrics import r2_score, mean_squared_error
nltk.download('stopwords')
from nltk.corpus import stopwords
from nltk.stem.porter import PorterStemmer
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


train_df = pd.read_csv('TrafficVolume_Train.csv')
train_df = prepare_df(train_df)
corpus = []
for i in range(0, len(train_df)):
  review = re.sub('[^a-zA-Z]', ' ', train_df['Weather Detail'][i])
  review = review.lower()
  review = review.split()
  ps = PorterStemmer()
  all_stopwords = stopwords.words('english')
  all_stopwords.remove('not')
  review = [ps.stem(word) for word in review if not word in set(all_stopwords)]
  review = ' '.join(review)
  corpus.append(review)
vectorizer = CountVectorizer()
vectorizer.fit(corpus)
pickle.dump(vectorizer, open("vector.pickel", "wb"))

train_df = pd.concat([train_df, pd.DataFrame(vectorizer.transform(corpus).toarray())], axis = 1)

(X_train, Y_train) = (train_df.drop(['Traffic Volume', 'Weather Detail', 'ID', 'Cloud'], axis = 1).values, train_df['Traffic Volume'].values)

rfreg = RandomForestRegressor(n_estimators = 90, max_depth = 20, min_samples_split = 5)
rfreg.fit(X_train, Y_train)
pickle.dump(rfreg, open("regressor", "wb"))