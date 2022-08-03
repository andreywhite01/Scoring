from logging.config import valid_ident
import pandas as pd
import numpy as np
from sklearn.preprocessing import OneHotEncoder
from imblearn.over_sampling import SMOTE, RandomOverSampler
from sklearn.preprocessing import MinMaxScaler
from sklearn.preprocessing import Normalizer
from sklearn.feature_selection import SelectKBest, chi2

def getMinMaxBorders(df, is_norm_map):
  min_borders = {}
  max_borders = {}

  for col in df.columns:
    q1 = np.quantile(df[col], 0.25)
    q2 = np.quantile(df[col], 0.75)
    if (is_norm_map[col] != 2):
      if (is_norm_map[col] == 1):
        max = q2 + 1.5*(q2-q1)
        max_borders[col] = max
      else:
        max = q2 + 3*(q2-q1)
        max_borders[col] = max
      min = q1 - 1.5*(q2-q1)
      min_borders[col] = min
    
  return [min_borders, max_borders]

def Normalization(df, is_norm_map, min_b, max_b):
  for col in df.columns:
    if (is_norm_map[col] != 2):
      if (max_b[col] - min_b[col] != 0):
        df[col] = (df[col] - min_b[col]) / (max_b[col] - min_b[col])
    
  return df

class DataPreprocessing():

    def __init__(self, df):
        self.df = df
        self.df['NANNUM'] = self.df.isna().sum(axis=1)
        self.replaceNanValuesMethod = 'AddNewFeatures'
        self.categoricalRecodingMethod = 'OneHotEncoder'
        self.normalizationMethod = 'Normalization'
        self.balancingMethod = 'SMOTE'
        self.test_size = 500
        self.validation_size = 500
        self.X_train = pd.DataFrame()
        self.y_train = pd.DataFrame()
        self.X_test = pd.DataFrame()
        self.y_test = pd.DataFrame()
        self.X_validation = pd.DataFrame()
        self.y_validation = pd.DataFrame()

    def ReplaceNanValues(self, method):
        if method == "meanAndMode":
            col_names_to_mode_replace = ['REASON','JOB', 'YOJ', 'DEROG', 'DELINQ', 'NINQ', 'CLNO']
            mode_features_values = dict(zip(col_names_to_mode_replace, 
                [self.df.REASON.mode()[0], 
                 self.df.JOB.mode()[0], 
                 self.df.YOJ.mode()[0], 
                 self.df.DEROG.mode()[0], 
                 self.df.DELINQ.mode()[0], 
                 self.df.NINQ.mode()[0], 
                 self.df.CLNO.mode()[0]]))

            col_names_to_mean_replace = ['LOAN','MORTDUE', 'VALUE', 'CLAGE', 'DEBTINC']
            mean_features_values = dict(zip(col_names_to_mean_replace, 
                [self.df.LOAN.mean(), 
                 self.df.MORTDUE.mean(), 
                 self.df.VALUE.mean(), 
                 self.df.CLAGE.mean(),
                 self.df.DEBTINC.mean()]))

            self.df[col_names_to_mode_replace] = self.df[col_names_to_mode_replace].fillna(mode_features_values)
            self.df[col_names_to_mean_replace] = self.df[col_names_to_mean_replace].fillna(mean_features_values)
            
        if method == "AddNewFeatures":
            self.df['MORT'] = self.df['MORTDUE'].isna() * 1.
            self.df['MORTDUE'] = self.df['MORTDUE'].fillna(self.df.MORTDUE.mean())

            self.df['POOR'] = self.df['VALUE'].isna() * 1.
            self.df['VALUE'] = self.df['VALUE'].fillna(self.df.VALUE.mean())

            self.df['REASONnun'] = self.df['REASON'].isna() * 1.
            self.df['REASON'] = self.df['REASON'].fillna(self.df.REASON.mode()[0])

            self.df['JOB'] = self.df['JOB'].fillna('Unemployed')

            self.df['YOJnun'] = self.df['YOJ'].isna() * 1.
            self.df['YOJ'] = self.df['YOJ'].fillna(self.df.YOJ.mode()[0])

            self.df['CLNOnun'] = self.df['CLNO'].isna() * 1.
            self.df['CLNO'] = self.df['CLNO'].fillna(0)

            self.df['DEROGnun'] = self.df['DEROG'].isna() * 1.
            self.df['DEROG'] = self.df['DEROG'].fillna(0)

            self.df['DELINQnun'] = self.df['DELINQ'].isna() * 1.
            self.df['DELINQ'] = self.df['DELINQ'].fillna(0)
            
            self.df['CLAGEnun'] = self.df['CLAGE'].isna() * 1.
            self.df['CLAGE'] = self.df['CLAGE'].fillna(0)

            self.df['NINQnun'] = self.df['NINQ'].isna() * 1.
            self.df['NINQ'] = self.df['NINQ'].fillna(self.df.NINQ.mean())

            self.df['INCOME'] = ~self.df['DEBTINC'].isna() * 1.
            self.df['DEBTINC'] = self.df['DEBTINC'].fillna(self.df['DEBTINC'].max())
    
    def CategoricalRecoding(self, method):
        if method == "OneHotEncoder":
            self.df['REASON'] = (np.array(self.df['REASON']) == 'DebtCon') * 1.

            ohe = OneHotEncoder(sparse = False)
            new_ohe_features = ohe.fit_transform(self.df.JOB.values.reshape(-1,1))
            tmp = pd.DataFrame(new_ohe_features, columns=['JOB=' + str(job) for job in range(new_ohe_features.shape[1])])
            self.df = pd.concat([self.df, tmp], axis=1, join='outer')
            self.df = self.df.drop(['JOB'], axis=1)
        
        if method == "ReplaceInPlace":
            dict_name_index = dict(zip(self.df.JOB.unique(), range(self.df.Job.nunique())))
            self.df[['JOB']] = self.df[['JOB']].replace(dict_name_index)
            self.df = self.df.drop(['JOB'], axis=1)
        
    def FeatureSelection(self, after_split, k=10):
        if after_split == False:
            selector = SelectKBest(chi2, k=k)
            X = self.df.drop(['BAD'],axis=1)
            y = self.df[['BAD']]
            selector.fit(X, y)
            X_new = pd.DataFrame(selector.transform(X), columns=X.columns[selector.get_support()])
            self.df = y
            self.df = pd.concat([X_new, self.df],axis=1)
        else:
            selector = SelectKBest(chi2, k=k)
            selector.fit(self.X_train, self.y_train)
            col_names = self.X_train.columns[selector.get_support()]
            self.X_train = pd.DataFrame(selector.transform(self.X_train), columns=col_names)
            self.X_test = pd.DataFrame(selector.transform(self.X_test), columns=col_names)
            self.X_validation = pd.DataFrame(selector.transform(self.X_validation), columns=col_names)
            y = self.df.BAD
            self.df = pd.DataFrame(selector.transform(self.df.drop(['BAD'],axis=1)), columns=col_names)
            self.df['BAD'] = y
        return self.df

    def TrainTestValSplit(self, test_size, validation_size):
        Data_test = self.df.sample(test_size + validation_size, random_state=42)                                             
        Data_train = self.df[~self.df.index.isin(Data_test.index)]

        Data_validation = Data_test.sample(validation_size, random_state=33)
        Data_test = Data_test[~Data_test.index.isin(Data_validation.index)]

        self.X_train = Data_train.drop(['BAD'], axis=1)
        self.y_train = Data_train.BAD

        self.X_test = Data_test.drop(['BAD'], axis=1)
        self.y_test = Data_test.BAD

        self.X_validation = Data_validation.drop(['BAD'], axis=1)
        self.y_validation = Data_validation.BAD

    def PlotFeatureDist(self):
        self.df.drop(['BAD'], axis=1).hist(figsize=(20,8), bins=30);

    def Normalization(self, method, is_norm_dist):
        if method == "MinMaxScaler":
            scaler = MinMaxScaler()
            self.X_train = scaler.fit_transform(self.X_train)
            self.X_test = scaler.fit_transform(self.X_test)
            self.X_validation = scaler.fit_transform(self.X_validation)

        if method == "Normalizer":
            scaler = Normalizer()
            self.X_train = scaler.fit_transform(self.X_train)
            self.X_test = scaler.fit_transform(self.X_test)
            self.X_validation = scaler.fit_transform(self.X_validation)
        
        if method == "Normalization":
            X = self.df.drop(['BAD'], axis=1)
            dict_is_norm_dist = dict(zip(self.df.drop(['BAD'],axis=1).columns, is_norm_dist))

            min_b, max_b = getMinMaxBorders(X, dict_is_norm_dist)

            self.X_train = Normalization(self.X_train, dict_is_norm_dist, min_b, max_b)
            self.X_test = Normalization(self.X_test, dict_is_norm_dist, min_b, max_b)
            self.X_validation = Normalization(self.X_validation, dict_is_norm_dist, min_b, max_b)
        return [min_b, max_b]
    def Balancing(self, method, test_val_part=0.2):
        if method == "ROS":
            ros = RandomOverSampler(random_state=0)
            self.X_train, self.y_train = ros.fit_resample(self.X_train, self.y_train)
            self.X_test, self.y_test = ros.fit_resample(self.X_test, self.y_test)
            self.X_validation, self.y_validation = ros.fit_resample(self.X_validation, self.y_validation)
            return
        if method == "SMOTE":
            smote = SMOTE(random_state=0)
            self.X_train, self.y_train = smote.fit_resample(self.X_train, self.y_train)
            self.X_test, self.y_test = smote.fit_resample(self.X_test, self.y_test)
            self.X_validation, self.y_validation = smote.fit_resample(self.X_validation, self.y_validation)
        if method == "SMOTE_SEP":
            df_with_nan = self.df.loc[self.df.NANNUM != 0]
            df_without_nan = self.df.loc[self.df.NANNUM == 0]

            #Разделим выборку, содержащую пропущенные значения
            test_val_len = int(df_with_nan.shape[0] * test_val_part)

            WN_Data_test = df_with_nan.sample(test_val_len, random_state=3)
            WN_Data_train = df_with_nan[~df_with_nan.index.isin(WN_Data_test.index)]
            WN_Data_validation = WN_Data_test.sample(test_val_len // 2, random_state=2)
            WN_Data_test = WN_Data_test[~WN_Data_test.index.isin(WN_Data_validation.index)] 
            
            WN_X_train = WN_Data_train.drop(['BAD'], axis=1)
            WN_y_train = WN_Data_train.BAD
            
            WN_X_test = WN_Data_test.drop(['BAD'], axis=1)
            WN_y_test = WN_Data_test.BAD
            
            WN_X_validation = WN_Data_validation.drop(['BAD'], axis=1)
            WN_y_validation = WN_Data_validation.BAD

            #Разделим выборку, не содержащую пропущенные значения
            test_val_len = int(df_without_nan.shape[0] * test_val_part)

            WON_Data_test = df_without_nan.sample(test_val_len, random_state=42)
            WON_Data_train = df_without_nan[~df_without_nan.index.isin(WON_Data_test.index)]
            WON_Data_validation = WON_Data_test.sample(test_val_len // 2, random_state=33)
            WON_Data_test = WON_Data_test[~WON_Data_test.index.isin(WON_Data_validation.index)]

            WON_X_train = WON_Data_train.drop(['BAD'], axis=1)
            WON_y_train = WON_Data_train.BAD

            WON_X_test = WON_Data_test.drop(['BAD'], axis=1)
            WON_y_test = WON_Data_test.BAD

            WON_X_validation = WON_Data_validation.drop(['BAD'], axis=1)
            WON_y_validation = WON_Data_validation.BAD
            
            smote = SMOTE(random_state=0)
            WN_X_train, WN_y_train = smote.fit_resample(WN_X_train, WN_y_train)
            WN_X_test, WN_y_test = smote.fit_resample(WN_X_test, WN_y_test)
            WN_X_validation, WN_y_validation = smote.fit_resample(WN_X_validation, WN_y_validation)
            
            WON_X_train, WON_y_train = smote.fit_resample(WON_X_train, WON_y_train)
            WON_X_test, WON_y_test = smote.fit_resample(WON_X_test, WON_y_test)
            WON_X_validation, WON_y_validation = smote.fit_resample(WON_X_validation, WON_y_validation)

            self.X_train = pd.concat([WN_X_train, WON_X_train])
            self.X_test = pd.concat([WN_X_test, WON_X_test])
            self.X_validation = pd.concat([WN_X_validation, WON_X_validation])

            self.y_train = pd.concat([WN_y_train, WON_y_train])
            self.y_test = pd.concat([WN_y_test, WON_y_test])
            self.y_validation = pd.concat([WN_y_validation, WON_y_validation])


    def GetDf(self):
        return self.df

    def GetTrain(self):
        return [self.X_train, self.y_train]

    def GetTest(self):
        return [self.X_test, self.y_test]

    def GetValidation(self):
        return [self.X_validation, self.y_validation]
    