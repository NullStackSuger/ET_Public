namespace ET;

[EntitySystemOf(typeof(NumericComponent))]
public static class NumericComponentSystem
{
    #region Get

    public static bool GetAsBool(this NumericComponent self, int numericType)
    {
        return self.GetByKey(numericType) != 0;
    }
    
    public static float GetAsFloat(this NumericComponent self, int numericType)
    {
        return self.GetByKey(numericType) / NumericConst.FloatRate;
    }
    
    public static long GetAsLong(this NumericComponent self, int numericType)
    {
        return self.GetByKey(numericType);
    }
    
    public static int GetAsInt(this NumericComponent self, int numericType)
    {
        return (int)self.GetByKey(numericType);
    }

    private static long GetByKey(this NumericComponent self, int numericType)
    {
        self.NumericDic.TryGetValue(numericType, out long value);
        return value;
    }

    private static bool TryGetByKey(this NumericComponent self, int numericType, out long value)
    {
        return self.NumericDic.TryGetValue(numericType, out value);
    }

    #endregion

    #region Set

    public static void Set(this NumericComponent self, int numericType, bool value)
    {
        self.ChangeByKey(numericType, value ? 1 : 0);
    }

    public static void Set(this NumericComponent self, int numericType, float value)
    {
        self.ChangeByKey(numericType, (long)(value * NumericConst.FloatRate));
    }
    
    public static void Set(this NumericComponent self, int numericType, long value)
    {
        self.ChangeByKey(numericType, value);   
    }
    
    public static void Set(this NumericComponent self, int numericType, int value)
    {
        self.ChangeByKey(numericType, value);
    }

    #endregion

    #region Change

    public static void Change(this NumericComponent self, int numericType, float value)
    {
        value += self.GetAsFloat(numericType);
        self.Set(numericType, value);
    }
    
    public static void Change(this NumericComponent self, int numericType, long value)
    {
        value += self.GetAsLong(numericType);
        self.Set(numericType, value);
    }
    
    public static void Change(this NumericComponent self, int numericType, int value)
    {
        value += self.GetAsInt(numericType);
        self.Set(numericType, value);
    }

    #endregion
    
    private static void ChangeByKey(this NumericComponent self, int numericType, long newValue)
    {
        long oldValue = self.GetByKey(numericType);
        if (oldValue == newValue) return;
        
        // 对结果进行限制
        self.NumericDic[numericType] = LimitValue(self, numericType, newValue);
        
        // 更新最终数值
        UpdateFinal(self, numericType);
        
        // 更新受影响数值 如1力量=2.2生命
        UpdateAffect(self, numericType, oldValue);
        
        // 广播数值改变
        EventSystem.Instance.Publish(self.Scene(), new NumericChange() { entity = self.Parent, numericType = numericType, oldValue = oldValue, newValue = newValue });

        static long LimitValue(NumericComponent self, int numericType, long newValue)
        {
            if (NumericLimitCategory.Instance.TryGet(numericType, out NumericLimitConfig config))
            {
                long min = config.Min;
                if (self.TryGetByKey((int)min, out long minTmp)) min = minTmp;
                long max = config.Max;
                if (self.TryGetByKey((int)max, out long maxTmp)) max = maxTmp;
                return Math.Clamp(newValue, min, max);
            }
            else
            {
                return newValue;
            }
        }
        
        static void UpdateFinal(NumericComponent self, int numericType)
        {
            if (!NumericComponent.IsGrow.Contains(numericType)) return;
            
            // eg.MaxHp0 = 100, MaxHp1 = 1001, MaxHp2 = 1002
            int final     = numericType / 10;
            int bas       = final * 10 + 1;
            int add       = final * 10 + 2;
            int pct       = final * 10 + 3;
            int finalAdd  = final * 10 + 4;
            int finalPct  = final * 10 + 5;
            int resultAdd = final * 10 + 6;
            
            // 第1步 = 基础 + 基础增加 (1+2)
            // var value1 = self.GetByKey(bas) + self.GetByKey(add);
            // 第2步 = * 百分比  (1+2)*3
            // var value2 = value1 * (NumericConst.IntRate + self.GetByKey(pct)) / NumericConst.IntRate;
            // 第3步 = + 最终额外增加 (1+2)*3+4
            // var value3 = value2 + self.GetByKey(finalAdd);
            // 第4步 = * 最终百分比 ((1+2)*3+4)*5
            // var value4 = value3 * (NumericConst.IntRate + self.GetByKey(finalPct)) / NumericConst.IntRate;
            // 第5步 = + 结果增加 ((1+2)*3+4)*5+6
            // var value5 = value4 + self.GetByKey(resultAdd);
            // 第6步 最终结果 向下取整 (C#中存在整除 所以可以不用取整)
            // var result = (long)value5;
            
            // 定点数: https://lib9kmxvq7k.feishu.cn/wiki/Gl1VwvLmlicjE8krUz0cxfcsnhc
            // ((((基础值 + 附加值) * 百分比因子) + 最终附加) * 最终百分比因子) + 结果附加
            
            var result =
                (
                    (

                        // 1 + 2
                        self.GetByKey(bas) + self.GetByKey(add)
                    )

                    // * 3
                    * (NumericConst.IntRate + self.GetByKey(pct)) / NumericConst.IntRate

                    // + 4
                    + self.GetByKey(finalAdd)
                )

                // * 5
                * (NumericConst.IntRate + self.GetByKey(finalPct)) / NumericConst.IntRate

                // + 6
                + self.GetByKey(resultAdd);
            
            self.ChangeByKey(final, result);
        }

        static void UpdateAffect(NumericComponent self, int numericType, long oldValue)
        {
            if (!NumericAffectCategory.Instance.TryGet(numericType, out NumericAffectConfig config)) return;
            
            foreach (int affect in config.Affects)
            {
                // Affects可能改变当前数值, 比如1力量=2.2生命, 生命<10时获得1力量, 所以不能提前获取newValue
                long affectValue = EventSystem.Instance.Invoke<NumericAffect, long>(GenerateUniqueId(numericType, affect), new NumericAffect()
                {
                    numericDic = self.NumericDic,

                    numericType = numericType,
                    oldValue = oldValue,
                    newValue = self.GetAsLong(numericType),

                    affectNumericType = affect,
                    affectValue = self.GetAsLong(affect),
                });
                
                self.ChangeByKey(affect, affectValue);
            }
        }
    }
    
    private static long GenerateUniqueId(int value1, int value2)
    {
        if (value1 <= 0 || value2 <= 0)
        {
            Console.WriteLine($"生成唯一ID失败: 值必须大于0 请检查: {value1}, {value2}");
            return 0;
        }
        return ((long)value1 << 32) | (value2 & 0xFFFFFFFFL);
    }

    /*#region Add

    public static Dictionary<int, long> Add(this NumericComponent self, Dictionary<int, long> target)
    {
        return AddInner([self.NumericDic, target]);
    }
    
    public static Dictionary<int, long> Add(this NumericComponent self, params Dictionary<int, long>[] targets)
    {
        return AddInner([self.NumericDic, AddInner(targets)]);
    }
    
    public static Dictionary<int, long> Add(this NumericComponent self, IEnumerable<Dictionary<int, long>> targets)
    {
        return AddInner([self.NumericDic, AddInner(targets)]);
    }
    
    public static Dictionary<int, long> Add(this NumericComponent self, NumericComponent target)
    {
        return AddInner([self.NumericDic, target.NumericDic]);
    }

    public static Dictionary<int, long> Add(this NumericComponent self, params NumericComponent[] targets)
    {
        return AddInner([self.NumericDic, AddInner(targets)]);
    }

    public static Dictionary<int, long> Add(this NumericComponent self, IEnumerable<NumericComponent> targets)
    {
        return AddInner([self.NumericDic, AddInner(targets)]);
    }

    private static Dictionary<int, long> AddInner(IEnumerable<Dictionary<int, long>> targets)
    {
        var dic = new Dictionary<int, long>();
        foreach (Dictionary<int,long> target in targets)
        {
            foreach ((int key, long value) in target)
            {
                if (!dic.TryAdd(key, value))
                {
                    dic[key] += value;
                }
            }
        }
        return dic;
    }

    private static Dictionary<int, long> AddInner(IEnumerable<NumericComponent> targets)
    {
        var dic = new Dictionary<int, long>();
        foreach (NumericComponent target in targets)
        {
            foreach ((int key, long value) in target.NumericDic)
            {
                if (!dic.TryAdd(key, value))
                {
                    dic[key] += value;
                }
            }
        }
        return dic;
    }
    
    public static void AddToSelf(this NumericComponent self, Dictionary<int, long> target)
    {
        self.NumericDic = self.Add(target);
        self.PublishAll();
    }

    public static void AddToSelf(this NumericComponent self, params Dictionary<int, long>[] targets)
    {
        self.NumericDic = self.Add(targets);
        self.PublishAll();
    }

    public static void AddToSelf(this NumericComponent self, IEnumerable<Dictionary<int, long>> targets)
    {
        self.NumericDic = self.Add(targets);
        self.PublishAll();
    }
    
    public static void AddToSelf(this NumericComponent self, NumericComponent target)
    {
        self.NumericDic = self.Add(target);
        self.PublishAll();
    }

    public static void AddToSelf(this NumericComponent self, params NumericComponent[] targets)
    {
        self.NumericDic = self.Add(targets);
        self.PublishAll();
    }

    public static void AddToSelf(this NumericComponent self, IEnumerable<NumericComponent> targets)
    {
        self.NumericDic = self.Add(targets);
        self.PublishAll();
    }

    #endregion

    public static void PublishAll(this NumericComponent self)
    {
        var dic = new Dictionary<int, long>(self.NumericDic);
        foreach ((int key, long value) in dic)
        {
            EventSystem.Instance.Publish(self.Scene(), new NumericChange() { entity = self, numericType = key, oldValue = 0, newValue = value });
        }
    }*/
}