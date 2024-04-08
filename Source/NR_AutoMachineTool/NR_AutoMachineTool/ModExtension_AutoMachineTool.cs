using System;
using Verse;

namespace NR_AutoMachineTool;

public class ModExtension_AutoMachineTool : DefModExtension
{
    public readonly int tier = 1;
    private IInputCellResolver inputCellResolver;

    public Type inputCellResolverType;

    private IOutputCellResolver outputCellResolver;

    public Type outputCellResolverType;

    private ITargetCellResolver targetCellResolver;

    public Type targetCellResolverType;

    public bool toUnderground;

    public bool underground;

    public ITargetCellResolver TargetCellResolver
    {
        get
        {
            if (targetCellResolverType == null)
            {
                return null;
            }

            if (targetCellResolver != null)
            {
                return targetCellResolver;
            }

            targetCellResolver = (ITargetCellResolver)Activator.CreateInstance(targetCellResolverType);
            targetCellResolver.Parent = this;

            return targetCellResolver;
        }
    }

    public IOutputCellResolver OutputCellResolver
    {
        get
        {
            if (outputCellResolverType == null)
            {
                return null;
            }

            if (outputCellResolver != null)
            {
                return outputCellResolver;
            }

            outputCellResolver = (IOutputCellResolver)Activator.CreateInstance(outputCellResolverType);
            outputCellResolver.Parent = this;

            return outputCellResolver;
        }
    }

    public IInputCellResolver InputCellResolver
    {
        get
        {
            if (inputCellResolverType == null)
            {
                return null;
            }

            if (inputCellResolver != null)
            {
                return inputCellResolver;
            }

            inputCellResolver = (IInputCellResolver)Activator.CreateInstance(inputCellResolverType);
            inputCellResolver.Parent = this;

            return inputCellResolver;
        }
    }
}