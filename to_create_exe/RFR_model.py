from pandas import read_csv
from json import load as json_load
from joblib import load as joblib_load

import os
import sys

def resource_path(relative_path):
    try:
        base_path = sys._MEIPASS
    except Exception:
        base_path = os.path.abspath(".")

    return os.path.join(base_path, relative_path)

def Normalization(df, min_borders, max_borders):
  for col in min_borders:
    if max_borders[col] - min_borders[col] == 0:
        continue
    df[col] = (df[col] - min_borders[col]) / (max_borders[col] - min_borders[col])
  return df

input_filename = input()

input = read_csv(input_filename)

with open(resource_path("ValsToRepaceNan.json"), "r") as read_file:
    ValsToRepaceNan = json_load(read_file)

input['MORTDUE'] = input['MORTDUE'].fillna(ValsToRepaceNan['MORTDUE'])

input['VALUE'] = input['VALUE'].fillna(ValsToRepaceNan['VALUE'])

input['DEROG'] = input['DEROG'].fillna(ValsToRepaceNan['DEROG'])

input['DELINQ'] = input['DELINQ'].fillna(ValsToRepaceNan['DELINQ'])

input['CLAGE'] = input['CLAGE'].fillna(ValsToRepaceNan['CLAGE'])

input['NINQ'] = input['NINQ'].fillna(ValsToRepaceNan['NINQ'])

input['INCOME'] = ~input.DEBTINC.isna() * 1.

input['DEBTINC'] = input['DEBTINC'].fillna(ValsToRepaceNan['DEBTINC'])

with open(resource_path("min_borders.json"), "r") as read_file:
    min_borders = json_load(read_file)
with open(resource_path("max_borders.json"), "r") as read_file:
    max_borders = json_load(read_file)

input = Normalization(input, min_borders, max_borders)

model = joblib_load(resource_path('RFR_model.joblib'))

prediction = model.predict(input)[0]

print(prediction)