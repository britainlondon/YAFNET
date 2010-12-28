﻿/* Yet Another Forum.NET
 * Copyright (C) 2006-2010 Jaben Cargman
 * http://www.yetanotherforum.net/
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 */
namespace YAF.Core
{
  #region Using

  using System;

  using Autofac;

  using YAF.Types;
  using YAF.Types.Interfaces;

  #endregion

  /// <summary>
  /// The auto fac service locator provider.
  /// </summary>
  public class AutoFacServiceLocatorProvider : IServiceLocator
  {
    #region Constructors and Destructors

    /// <summary>
    /// Initializes a new instance of the <see cref="AutoFacServiceLocatorProvider"/> class.
    /// </summary>
    /// <param name="container">
    /// The container.
    /// </param>
    public AutoFacServiceLocatorProvider([NotNull] ILifetimeScope container)
    {
      CodeContracts.ArgumentNotNull(container, "container");

      this.Container = container;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets Container.
    /// </summary>
    public ILifetimeScope Container { get; set; }

    #endregion

    #region Implemented Interfaces

    #region IServiceLocator

    /// <summary>
    /// The get.
    /// </summary>
    /// <param name="serviceType">
    /// The service type.
    /// </param>
    /// <returns>
    /// The get.
    /// </returns>
    public object Get(Type serviceType)
    {
      CodeContracts.ArgumentNotNull(serviceType, "serviceType");

      return this.Container.Resolve(serviceType);
    }

    /// <summary>
    /// The get.
    /// </summary>
    /// <param name="serviceType">
    /// The service type.
    /// </param>
    /// <param name="named">
    /// The named.
    /// </param>
    /// <returns>
    /// The get.
    /// </returns>
    public object Get(Type serviceType, string named)
    {
      CodeContracts.ArgumentNotNull(serviceType, "serviceType");
      CodeContracts.ArgumentNotNull(named, "named");

      return this.Container.ResolveNamed(named, serviceType);
    }

    /// <summary>
    /// The try get.
    /// </summary>
    /// <param name="serviceType">
    /// The service type.
    /// </param>
    /// <param name="instance">
    /// The instance.
    /// </param>
    /// <returns>
    /// The try get.
    /// </returns>
    public bool TryGet(Type serviceType, [NotNull] out object instance)
    {
      CodeContracts.ArgumentNotNull(serviceType, "serviceType");

      return this.Container.TryResolve(out instance);
    }

    /// <summary>
    /// The try get.
    /// </summary>
    /// <param name="serviceType">
    /// The service type.
    /// </param>
    /// <param name="named">
    /// The named.
    /// </param>
    /// <param name="instance">
    /// The instance.
    /// </param>
    /// <returns>
    /// The try get.
    /// </returns>
    public bool TryGet(Type serviceType, string named, [NotNull] out object instance)
    {
      CodeContracts.ArgumentNotNull(serviceType, "serviceType");
      CodeContracts.ArgumentNotNull(named, "named");

      return this.Container.TryResolve(out instance);
    }

    #endregion

    #endregion
  }
}