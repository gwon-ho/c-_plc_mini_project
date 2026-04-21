// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz

namespace realvirtual
{
    //! Interface for components whose physics can be externally time-synced (e.g. by Siemens Simit)
    public interface ITimeSyncedPhysics
    {
        //! Called with external deltaTime instead of Time.fixedDeltaTime
        void CalcFixedUpdate(float deltaTime);
    }
}
