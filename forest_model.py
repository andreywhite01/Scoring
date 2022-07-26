from pandas import read_csv
from joblib import load as joblib_load
from json import load as json_load
import pickle

def Normalization(df, min_borders, max_borders):
  for col in min_borders:
    if max_borders[col] - min_borders[col] == 0:
        continue
    df[col] = (df[col] - min_borders[col]) / (max_borders[col] - min_borders[col])
  return df

input = read_csv('input_.csv')


with open("mean_values.json", "r") as read_file:
    mean_values = json_load(read_file)

input['MORT'] = input['MORTDUE'].isna() * 1.
input['MORTDUE'] = input['MORTDUE'].fillna(mean_values['MORTDUE'])

input['POOR'] = input['VALUE'].isna() * 1.
input['VALUE'] = input['VALUE'].fillna(mean_values['VALUE'])

input['REASON'] = input['REASON'].fillna('DebtCon')

input['JOB'] = input['JOB'].fillna('Unemployed')

input['YOJ'] = input['YOJ'].fillna(mean_values['YOJ'])

input['CLNO'] = input['CLNO'].fillna(16)

input['DEROGnun'] = input['DEROG'].isna() * 1.
input['DEROG'] = input['DEROG'].fillna(0)

input['DELINQnun'] = input['DELINQ'].isna() * 1.
input['DELINQ'] = input['DELINQ'].fillna(0)

input['CLAGE'] = input['CLAGE'].fillna(mean_values['CLAGE'])

input['NINQ'] = input['NINQ'].fillna(mean_values['NINQ'])

input['INCOME'] = ~input.DEBTINC.isna() * 1.

input['DEBTINC'] = input['DEBTINC'].fillna(mean_values['DEBTINC'])

input['REASON'] = (input['REASON'].values[0] == 'DebtCon') * 1.

job_name_from_form = input['JOB']

with open("jobs.json", "r") as read_file:
    jobs = json_load(read_file)

for i in range(len(jobs)):
    input[f'JOB={i}'] = [0]

input[f'JOB={jobs[str(job_name_from_form.values[0])]}'] = [1]

input = input.drop(['JOB'], axis=1)

with open("min_borders.json", "r") as read_file:
    min_borders = json_load(read_file)
with open("max_borders.json", "r") as read_file:
    max_borders = json_load(read_file)

input = Normalization(input, min_borders, max_borders)

#model = joblib_load('filename.joblib')

with open('model.joblib', 'rb') as file:
    model = joblib_load(file)

prediction = model.predict(input)[0]

print(prediction)