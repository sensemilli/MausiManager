using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using MathNet.Numerics;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.PnCommands;

namespace WiCAM.Pn4000.PN3D.Doc;

public class PnBndDocBendHandler
{
    [CompilerGenerated]
    private sealed class _003CAllOrders_003Ed__0 : IEnumerable<(IBendSequenceOrder? strategy, int bruteforceIndex)>, IEnumerable, IEnumerator<(IBendSequenceOrder? strategy, int bruteforceIndex)>, IEnumerator, IDisposable
    {
        private int _003C_003E1__state;

        private (IBendSequenceOrder? strategy, int bruteforceIndex) _003C_003E2__current;

        private int _003C_003El__initialThreadId;

        private IDoc3d doc;

        public IDoc3d _003C_003E3__doc;

        private int _003CbendAmount_003E5__2;

        private int _003Ci_003E5__3;

        private IEnumerator<IBendSequenceOrder> _003C_003E7__wrap3;

        (IBendSequenceOrder? strategy, int bruteforceIndex) IEnumerator<(IBendSequenceOrder? strategy, int bruteforceIndex)>.Current
        {
            [DebuggerHidden]
            get
            {
                return this._003C_003E2__current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this._003C_003E2__current;
            }
        }

        [DebuggerHidden]
        public _003CAllOrders_003Ed__0(int _003C_003E1__state)
        {
            this._003C_003E1__state = _003C_003E1__state;
            this._003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
        }

        [DebuggerHidden]
        void IDisposable.Dispose()
        {
            int num = this._003C_003E1__state;
            if (num == -3 || num == 2)
            {
                try
                {
                }
                finally
                {
                    this._003C_003Em__Finally1();
                }
            }
            this._003C_003E7__wrap3 = null;
            this._003C_003E1__state = -2;
        }

        private bool MoveNext()
        {
            try
            {
                switch (this._003C_003E1__state)
                {
                    default:
                        return false;
                    case 0:
                        {
                            this._003C_003E1__state = -1;
                            IBendMachine bendMachine = this.doc.BendMachine;
                            this._003CbendAmount_003E5__2 = this.doc.CombinedBendDescriptors.Sum((ICombinedBendDescriptorInternal x) => x.Enumerable.Count());
                            if (this._003CbendAmount_003E5__2 <= bendMachine.ToolCalculationSettings.BruteforceBendOrderAmount && !(from x in this.doc.CombinedBendDescriptors.SelectMany((ICombinedBendDescriptorInternal x) => x.Enumerable.Select((IBendDescriptor e) => e.BendParams.EntryFaceGroup.ID))
                                                                                                                                    group x by x).Any((IGrouping<int, int> g) => g.Count() > 1))
                            {
                                this._003Ci_003E5__3 = 0;
                                goto IL_0137;
                            }
                            this._003C_003E7__wrap3 = bendMachine.ToolCalculationSettings.BendOrderStrategies.Where((IBendSequenceOrder x) => x.Enabled).GetEnumerator();
                            this._003C_003E1__state = -3;
                            goto IL_01c2;
                        }
                    case 1:
                        this._003C_003E1__state = -1;
                        this._003Ci_003E5__3++;
                        goto IL_0137;
                    case 2:
                        {
                            this._003C_003E1__state = -3;
                            goto IL_01c2;
                        }
                    IL_0137:
                        if ((double)this._003Ci_003E5__3 < SpecialFunctions.Factorial(this._003CbendAmount_003E5__2))
                        {
                            this._003C_003E2__current = (strategy: null, bruteforceIndex: this._003Ci_003E5__3);
                            this._003C_003E1__state = 1;
                            return true;
                        }
                        break;
                    IL_01c2:
                        if (this._003C_003E7__wrap3.MoveNext())
                        {
                            this._003C_003E2__current = (strategy: this._003C_003E7__wrap3.Current, bruteforceIndex: -1);
                            this._003C_003E1__state = 2;
                            return true;
                        }
                        this._003C_003Em__Finally1();
                        this._003C_003E7__wrap3 = null;
                        break;
                }
                return false;
            }
            catch
            {
                //try-fault
                ((IDisposable)this).Dispose();
                throw;
            }
        }

        bool IEnumerator.MoveNext()
        {
            //ILSpy generated this explicit interface implementation from .override directive in MoveNext
            return this.MoveNext();
        }

        private void _003C_003Em__Finally1()
        {
            this._003C_003E1__state = -1;
            if (this._003C_003E7__wrap3 != null)
            {
                this._003C_003E7__wrap3.Dispose();
            }
        }

        [DebuggerHidden]
        void IEnumerator.Reset()
        {
            throw new NotSupportedException();
        }

        [DebuggerHidden]
        IEnumerator<(IBendSequenceOrder? strategy, int bruteforceIndex)> IEnumerable<(IBendSequenceOrder? strategy, int bruteforceIndex)>.GetEnumerator()
        {
            _003CAllOrders_003Ed__0 _003CAllOrders_003Ed__;
            if (this._003C_003E1__state == -2 && this._003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
            {
                this._003C_003E1__state = 0;
                _003CAllOrders_003Ed__ = this;
            }
            else
            {
                _003CAllOrders_003Ed__ = new _003CAllOrders_003Ed__0(0);
            }
            _003CAllOrders_003Ed__.doc = this._003C_003E3__doc;
            return _003CAllOrders_003Ed__;
        }

        [DebuggerHidden]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<(IBendSequenceOrder? strategy, int bruteforceIndex)>)this).GetEnumerator();
        }
    }

    [IteratorStateMachine(typeof(_003CAllOrders_003Ed__0))]
    private static IEnumerable<(IBendSequenceOrder? strategy, int bruteforceIndex)> AllOrders(IDoc3d doc)
    {
        //yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
        return new _003CAllOrders_003Ed__0(-2)
        {
            _003C_003E3__doc = doc
        };
    }

    public static F2exeReturnCode SimulationPlay(IDoc3d doc)
    {
        if (doc == null)
        {
            return F2exeReturnCode.ERROR_NO_DATA;
        }
        doc.BendSimulation.Play();
        doc.HasSimulationCalculated = true;
        return F2exeReturnCode.OK;
    }

    public static F2exeReturnCode SimulationPause(IDoc3d doc)
    {
        if (doc == null)
        {
            return F2exeReturnCode.ERROR_NO_DATA;
        }
        doc.BendSimulation.Pause();
        return F2exeReturnCode.OK;
    }

    public static F2exeReturnCode SimulationNext(IDoc3d doc)
    {
        if (doc == null)
        {
            return F2exeReturnCode.ERROR_NO_DATA;
        }
        doc.BendSimulation.GotoNextStep();
        return F2exeReturnCode.OK;
    }

    public static F2exeReturnCode SimulationEnd(IDoc3d doc)
    {
        if (doc == null)
        {
            return F2exeReturnCode.ERROR_NO_DATA;
        }
        doc.BendSimulation.GoToEnd();
        return F2exeReturnCode.OK;
    }

    public static F2exeReturnCode SimulationPrevious(IDoc3d doc)
    {
        if (doc == null)
        {
            return F2exeReturnCode.ERROR_NO_DATA;
        }
        doc.BendSimulation.GotoPrevStep();
        return F2exeReturnCode.OK;
    }

    public static F2exeReturnCode SimulationStart(IDoc3d doc)
    {
        if (doc == null)
        {
            return F2exeReturnCode.ERROR_NO_DATA;
        }
        doc.BendSimulation.GotoStep(0.0);
        return F2exeReturnCode.OK;
    }

    public static F2exeReturnCode SimulationGotoStep(IDoc3d doc, double step, double maxStep)
    {
        if (doc == null)
        {
            return F2exeReturnCode.ERROR_NO_DATA;
        }
        doc.BendSimulation.GotoStep(step, maxStep);
        return F2exeReturnCode.OK;
    }
}
