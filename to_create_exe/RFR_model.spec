# -*- mode: python ; coding: utf-8 -*-


block_cipher = None


a = Analysis(
    ['RFR_model.py'],
    pathex=['C:\\Users\\71380279\\Desktop\\EXE_RFR'],
    binaries=[],
    datas=[],
    hiddenimports=['sklearn','sklearn.ensemble._forest'],
    hookspath=[],
    hooksconfig={},
    runtime_hooks=[],
    excludes=[],
    win_no_prefer_redirects=False,
    win_private_assemblies=False,
    cipher=block_cipher,
    noarchive=False,
)

a.datas += [('max_borders.json','C:\\Users\\71380279\\Desktop\\EXE_RFR\\max_borders.json', "DATA")]
a.datas += [('min_borders.json','C:\\Users\\71380279\\Desktop\\EXE_RFR\\min_borders.json', "DATA")]
a.datas += [('ValsToRepaceNan.json','C:\\Users\\71380279\\Desktop\\EXE_RFR\\ValsToRepaceNan.json', "DATA")]
a.datas += [('RFR_model.joblib','C:\\Users\\71380279\\Desktop\\EXE_RFR\\RFR_model.joblib', "DATA")]

pyz = PYZ(a.pure, a.zipped_data, cipher=block_cipher)

exe = EXE(
    pyz,
    a.scripts,
    a.binaries,
    a.zipfiles,
    a.datas,
    [],
    name='RFR_model',
    debug=False,
    bootloader_ignore_signals=False,
    strip=False,
    upx=True,
    upx_exclude=[],
    runtime_tmpdir=None,
    console=False,
    disable_windowed_traceback=False,
    argv_emulation=False,
    target_arch=None,
    codesign_identity=None,
    entitlements_file=None,
)
