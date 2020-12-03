public enum GameMode
{
    TDM,
    FFA,
    CTF,
#if BDGM
    SND,
#endif
#if CP
    CP,
#endif
#if GR
    GR,
#endif
#if LMS
    LSM,
#endif
#if ELIM
    ELIM,
#endif
    DM,
}